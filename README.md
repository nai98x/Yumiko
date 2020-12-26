# Yumiko
## Bot para Discord desarrollado en DSharpPlus - .NET Framework 
[![CodeFactor](https://www.codefactor.io/repository/github/nai98x/yumiko/badge?s=92181f030fc6101fb54afa74167809713aa4d060)](https://www.codefactor.io/repository/github/nai98x/yumiko)

Bot multi propósito con juegos, utilidades y memes. Visita su página: https://yumiko.uwu.ai/

## Instalación

### 1. Visual Studio
Lo primero a realizar es descargar el visual studio community desde su página web oficial, las opciones de instalación son las siguientes:
- Desarrollo de ASP.NET y web
- Desarrollo de escritorio de .NET
- Desarrollo de la plataforma universal de Windows
- Almacenamiento y procesamiento de datos

### 2. Clonar el repositorio y abrir la solución

### 3. SQL Server
- Instalar SQL Server
- Instalar un manejador (recomiendo SQL Server Express)
- Crear base de datos llamada: Yumiko
- Correr el siguiente script: [modelo.edmx.sql](YumikoBot/Data Access Layer/modelo.edmx.sql)

### 4. Instalar paquetes NuGet
- Ir a Administrar paquetes NuGet de la solución
- Activar: Incluir versión preliminar
- Instalar paquetes NuGet faltantes (los de DSharpPlus)

### 5. Configuraciones del bot
- Ir al archivo [config_ejemplo.json](YumikoBot/config_ejemplo.json) y renombrarlo a config.json
- Reemplazar TOKEN_DISCORD_BOT por tu token de bot de Discord
- Reemplazar el valor data source de la connection string en [App.config](YumikoBot/App.config) por la que tengas configurada

### Listo!
