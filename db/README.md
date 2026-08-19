# Base de datos

Esquema y stored procedures versionados. La base es **database-first**: el esquema se diseña acá (es
la fuente de verdad) y el código C# se adapta. El acceso desde el bot es vía **Dapper invocando
stored procedures** — nunca SQL embebido en el código.

Es el mismo criterio que AnilistConEnie, con una diferencia: acá **tablas, columnas y funciones van
en inglés**, como el resto del código de este repo.

## Estructura

```
db/
  schema/        # tablas, índices, constraints
  procedures/    # un .sql por stored procedure
```

## Tablas

| Tabla | Qué guarda |
|---|---|
| `anilist_users` | Vínculo entre una cuenta de Discord y una de AniList. Global, no por guild |
| `higher_or_lower_scores` | Récord de Higher or Lower por usuario y guild |
| `quiz_stats` | Stats acumuladas de trivia por usuario, guild, modo de juego y dificultad |

Dos particularidades de `quiz_stats`:

- `accuracy_percentage` se guarda calculado con **división entera** sobre los totales acumulados.
  Pasarlo a decimal re-rankea todos los leaderboards que ya existen.
- En modo `Genres`, la columna `difficulty` guarda el **nombre del género** en lugar de una
  dificultad.

`gamemode` y `difficulty` guardan los nombres de los enums de C# (`Characters`, `Easy`, …), nunca
las etiquetas en español que se muestran en los embeds.

## Convenciones de los scripts

- **Idempotentes**: cada script se puede correr más de una vez sin romper (`CREATE TABLE IF NOT
  EXISTS`, `CREATE OR REPLACE FUNCTION`).
- **Un stored procedure por archivo** en `procedures/`, con el nombre del SP como nombre de archivo.
- Cambios de esquema y los SPs que los consumen van en el **mismo commit** que el código C# que los
  usa.
- **Las migraciones no se versionan**: `schema/` refleja siempre el estado actual de cada tabla (el
  `CREATE` limpio), sin `ALTER` ni scripts de datos.

## Aplicar los scripts

Conectado a la base (por DBeaver o `psql`), correr primero los de `schema/` y después los de
`procedures/`. Como son idempotentes, reaplicarlos sincroniza la base con lo versionado.

```bash
for f in db/schema/*.sql db/procedures/*.sql; do psql -d yumiko -f "$f"; done
```

## Seguridad del rol del bot

El usuario con el que se conecta el bot no debe ser superuser ni dueño del schema: alcanza con
`CONNECT` a la base, `USAGE` del schema y `EXECUTE` sobre las funciones de `procedures/` (más los
permisos de tabla que esas funciones necesiten). La base escucha solo en localhost.

## AnilistConEnie

AnilistConEnie escribe el vínculo de AniList en `anilist_users`. Se conecta con un rol propio que
solo tiene `EXECUTE` sobre `anilist_user_upsert`:

```sql
CREATE ROLE anilistconenie LOGIN PASSWORD '...';
GRANT CONNECT ON DATABASE yumiko TO anilistconenie;
GRANT USAGE ON SCHEMA public TO anilistconenie;
GRANT EXECUTE ON FUNCTION anilist_user_upsert(bigint, integer) TO anilistconenie;
GRANT INSERT, UPDATE ON anilist_users TO anilistconenie;
```
