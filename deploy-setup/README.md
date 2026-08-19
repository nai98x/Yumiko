# Deploy

El bot se deploya por **CI/CD en GitHub Actions** (`.github/workflows/deploy.yml`). El job corre en
un runner **hosted de GitHub** (`ubuntu-latest`): `push` a `master` → restore → escaneo de paquetes
vulnerables → build → test → `dotnet publish -r linux-arm64 --no-self-contained` → empaqueta un
`publish.tar.gz` → lo copia al server por **SCP** → lo descomprime en `~/bots/Yumiko-app` y reinicia
por **SSH** con `systemctl --user`.

El proceso lo administra **systemd (servicio de usuario)**: sobrevive a la sesión SSH del deploy,
reinicia solo y loguea en journald.

## Secrets

Ninguno se versiona. Todos se resuelven por el **mismo mecanismo** (`IConfiguration`): en el
servidor por variable de entorno (las setea el unit de systemd), en local por User Secrets.

| Clave | Obligatorio | Para qué                              |
|---|---|---------------------------------------|
| `discordToken` | sí | Token del bot de Discord              |
| `ConnectionStrings:Database` | sí | Base de datos                         |
| `openWeatherMapToken` | sí | `/weather`                            |
| `theCatApiToken` | sí | `/cat`                                |
| `theDogApiToken` | sí | `/dog`                                |
| `AnilistApiClientId` | sí | URL de OAuth de `/anilist setprofile` |
| `topggToken` | no | Publicar stats y leer votos en top.gg |

Los seis obligatorios se validan al arrancar: si falta cualquiera, el proceso falla de entrada en
vez de reventar cuando alguien usa el comando. Además, si la base no responde cuando terminan de
bajar los guilds, el bot queda marcado como no inicializado y contesta que no está listo en vez de
fallar comando por comando.

Los ids de guild y canal y `Website` **no son secretos** y van en `appsettings.json`.

## Setup en local (una sola vez)

```bash
dotnet user-secrets --project src/Yumiko.Bot set discordToken "TU_TOKEN"
dotnet user-secrets --project src/Yumiko.Bot set "ConnectionStrings:Database" 'Host=...;Port=...;Database=...;Username=...;Password=...'
dotnet user-secrets --project src/Yumiko.Bot set openWeatherMapToken "..."
dotnet user-secrets --project src/Yumiko.Bot set theCatApiToken "..."
dotnet user-secrets --project src/Yumiko.Bot set theDogApiToken "..."
dotnet user-secrets --project src/Yumiko.Bot set AnilistApiClientId "..."
```

En local la base se alcanza por un túnel SSH contra el server, igual que en AnilistConEnie.

> Si algún valor tiene `$`, usá comillas **simples**: en fish/bash las dobles lo expanden y guardan
> el secret incompleto.

En local, el build **Debug** registra los comandos solo en el guild de `Ids:LogGuildId`, así que se
puede probar sin tocar la instancia pública. Nunca correr un build Release contra el token de
producción hasta haber hecho el smoke test completo.

## Setup en el server (una sola vez)

1. Runtime y layout. El publish es `--no-self-contained`, así que el server necesita el **runtime de
   .NET 10** (`dotnet --list-runtimes` tiene que mostrar `Microsoft.NETCore.App 10.x`). Si ya corre
   AnilistConEnie ahí, está.

   Este layout (`~/bots/...`) reemplaza al viejo, que era un clon del repo en `~/Yumiko` corriendo
   con `nohup`. Antes del primer deploy hay que bajar esa instancia (`pkill Yumiko`) y sacarla de
   cualquier arranque automático, o quedan dos bots con el mismo token.

2. Crear directorios estables:

   ```bash
   mkdir -p ~/bots/secrets ~/bots/Yumiko-app
   ```

3. Crear la base de datos. PostgreSQL ya corre en el server para AnilistConEnie: Yumiko usa la
   **misma instancia con base y rol propios**, para que un bot no pueda tocar los datos del otro.
   Como `postgres` (`sudo -u postgres psql`):

   ```sql
   CREATE ROLE bot_yumiko LOGIN PASSWORD 'GENERAR_UNA';
   CREATE DATABASE yumiko OWNER bot_yumiko;
   ```

   El rol del bot no debe ser superuser. Con ser dueño de su propia base alcanza; si se prefiere
   separar aún más, se puede crear la base con otro dueño y darle a `bot_yumiko` solo `CONNECT`,
   `USAGE` del schema y `EXECUTE` sobre las funciones de `db/procedures/` más los permisos de
   tabla que esas funciones usan.

   Aplicar el esquema y los stored procedures (idempotentes, se pueden reaplicar):

   ```bash
   for f in db/schema/*.sql db/procedures/*.sql; do psql -d yumiko -f "$f"; done   # o a mano por DBeaver
   ```

   Igual que con AnilistConEnie: PostgreSQL tiene que escuchar **solo** en localhost
   (`listen_addresses = 'localhost'`, verificable con `ss -tlnp | grep 5432`) y `pg_hba.conf` exigir
   `scram-sha-256` para conexiones locales. Al estar el bot en la misma máquina no hace falta TLS; si
   la base se mudara a otro host, agregar `SSL Mode=Require` a la connection string.

   Conviene sumar la base nueva al backup diario que ya existe para AnilistConEnie
   (`deploy/backup-db.sh` de ese repo): hoy solo dumpea la suya.

4. Colocar en `~/bots/secrets/` el archivo `yumiko.env`, que **no se versiona**. Acá lo lee systemd,
   no un shell: los valores van literales, sin comillas.

   ```
   discordToken=...
   ConnectionStrings__Database=Host=...;Port=...;Database=...;Username=...;Password=...
   openWeatherMapToken=...
   theCatApiToken=...
   theDogApiToken=...
   AnilistApiClientId=...
   topggToken=...
   ```

   Permisos: `chmod 700 ~/bots/secrets && chmod 600 ~/bots/secrets/*`. Solo el usuario que corre el
   servicio debe poder leerlos.

5. Instalar el servicio (`yumiko.service` de este directorio):

   ```bash
   cp deploy-setup/yumiko.service ~/.config/systemd/user/
   loginctl enable-linger "$USER"        # arranca sin sesión iniciada
   systemctl --user daemon-reload
   systemctl --user enable yumiko
   ```

6. En GitHub (Settings → Secrets and variables → Actions), configurar los secrets del deploy:
   - `HOST` — host o IP del server.
   - `USERNAME` — usuario SSH (el mismo que corre el servicio de systemd).
   - `PRIVATE_KEY` — clave privada SSH con acceso a ese usuario.

## Operación

```bash
systemctl --user status yumiko
journalctl --user -u yumiko -f
systemctl --user restart yumiko
```

El deploy preserva la carpeta `logs/` del directorio de la app: Serilog escribe ahí un archivo por
día y `/owner logs` devuelve el más reciente.

Si PostgreSQL no está arriba cuando el bot termina de bajar los guilds, el bot **no se cae**: queda
marcado como no inicializado (loguea `Could not connect to the database`) y responde que no está
listo. No reintenta solo: hay que levantar la base y hacer `systemctl --user restart yumiko`.

## Notas

- El publish es para **`linux-arm64`**. Si el server no es ARM64, cambiar el `-r` del workflow (y la
  arquitectura afecta también a los native assets de SkiaSharp).
- La primera vez conviene hacer una corrida en seco: apuntar el workflow a `~/bots/Yumiko-app-test`
  con un `yumiko-test.service` antes de tocar el unit vivo.
