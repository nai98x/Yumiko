# Yumiko
## Bot para Discord desarrollado en DSharpPlus - .NET Core 
[![CodeFactor](https://www.codefactor.io/repository/github/nai98x/yumiko/badge?s=92181f030fc6101fb54afa74167809713aa4d060)](https://www.codefactor.io/repository/github/nai98x/yumiko)
[![DigitalOcean Referral Badge](https://top.gg/api/widget/status/295182825521545218.svg)](https://top.gg/bot/295182825521545218)

Bot multi propósito con juegos, utilidades y consultas en AniList. Visita su página: https://yumiko.uwu.ai/

## Instalación

### 1. Visual Studio
Lo primero a realizar es descargar el visual studio community desde su página web oficial, las opciones de instalación son las siguientes:
- Desarrollo de ASP.NET y web
- Desarrollo de Azure
- Desarrollo de escritorio de .NET
- Desarrollo de la plataforma universal de Windows
- Desarrollo multiplataforma de .NET Core

### 2. Clonar el repositorio y abrir la solución

### 3. Instalar paquetes NuGet
- Ir a Administrar paquetes NuGet de la solución
- Activar: Incluir versión preliminar
- Instalar paquetes NuGet faltantes

### 4. Configurar Firebase
- Ir al [sitio web](https://firebase.google.com/) de Firebase
- Crear proyecto en caso de no tener (Con el plan Spark que es gratuito alcanza)
- Ir a Firestore y crear base de datos en modo test
- Quitar regla de expiracion del testeo
- Ir a Proyect settings -> Service accounts y generar una clave privada

### 5. Configuraciones del bot
- Ir al archivo [config_ejemplo.json](YumikoBot/config_ejemplo.json) y renombrarlo a config.json
- Ir al archivo [firebase_ejemplo.json](YumikoBot/firebase_ejemplo.json) y renombrarlo a firebase.json
- Configurar config.json y firebase.json con los datos correspondientes (los datos de firebase.json son los obtenidos del paso 4)

### Listo!
