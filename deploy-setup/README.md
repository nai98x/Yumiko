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

| Clave | Obligatorio | Para qué |
|---|---|---|
| `discordToken` | sí | Token del bot de Discord |
| `FIREBASE_CREDENTIALS_DIR` | sí | Carpeta que contiene el `firebase-yumiko.json` (service account de Firestore) |
| `openWeatherMapToken` | sí | `/weather` |
| `theCatApiToken` | sí | `/cat` |
| `theDogApiToken` | sí | `/dog` |
| `AnilistApiClientId` | sí | URL de OAuth de `/anilist setprofile` |
| `topggToken` | no | Publicar stats y leer votos en top.gg |

Los seis obligatorios se validan al arrancar: si falta cualquiera, el proceso falla de entrada en
vez de reventar cuando alguien usa el comando.

Los ids de guild y canal y `Website` **no son secretos** y van en `appsettings.json`.

## Setup en local (una sola vez)

```bash
dotnet user-secrets --project src/Yumiko.Bot set discordToken "TU_TOKEN"
dotnet user-secrets --project src/Yumiko.Bot set FIREBASE_CREDENTIALS_DIR /ruta/con/firebase-yumiko.json
dotnet user-secrets --project src/Yumiko.Bot set openWeatherMapToken "..."
dotnet user-secrets --project src/Yumiko.Bot set theCatApiToken "..."
dotnet user-secrets --project src/Yumiko.Bot set theDogApiToken "..."
dotnet user-secrets --project src/Yumiko.Bot set AnilistApiClientId "..."
```

El archivo de credenciales tiene que llamarse exactamente `firebase-yumiko.json` y vivir dentro de
`FIREBASE_CREDENTIALS_DIR`; su forma es la de `firebase-example.json` de este directorio. El
`project_id` sale de ahí adentro, no de configuración aparte.

Es **el mismo archivo y el mismo nombre** que espera AnilistConEnie, que lee esta misma base de
Firestore: si los dos bots corren en la misma máquina, comparten `~/bots/secrets/firebase-yumiko.json`
y hay una sola credencial que rotar.

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

3. Colocar en `~/bots/secrets/` lo que **no se versiona**:
   - `firebase-yumiko.json` — service account de Firestore. Si AnilistConEnie ya está deployado ahí,
     el archivo ya existe: no hay que duplicarlo.
   - `yumiko.env` con el resto de los secrets. Acá lo lee systemd, no un shell: los valores van
     literales, sin comillas.

     ```
     discordToken=...
     openWeatherMapToken=...
     theCatApiToken=...
     theDogApiToken=...
     AnilistApiClientId=...
     topggToken=...
     ```

   Permisos: `chmod 700 ~/bots/secrets && chmod 600 ~/bots/secrets/*`. Solo el usuario que corre el
   servicio debe poder leerlos.

   `FIREBASE_CREDENTIALS_DIR` no va en el `.env`: lo setea el propio unit, apuntando a esa carpeta.

4. Instalar el servicio (`yumiko.service` de este directorio):

   ```bash
   cp deploy-setup/yumiko.service ~/.config/systemd/user/
   loginctl enable-linger "$USER"        # arranca sin sesión iniciada
   systemctl --user daemon-reload
   systemctl --user enable yumiko
   ```

5. En GitHub (Settings → Secrets and variables → Actions), configurar los secrets del deploy:
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

## Notas

- El publish es para **`linux-arm64`**. Si el server no es ARM64, cambiar el `-r` del workflow (y la
  arquitectura afecta también a los native assets de SkiaSharp).
- La primera vez conviene hacer una corrida en seco: apuntar el workflow a `~/bots/Yumiko-app-test`
  con un `yumiko-test.service` antes de tocar el unit vivo.
