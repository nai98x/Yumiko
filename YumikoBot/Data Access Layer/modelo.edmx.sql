
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/01/2020 20:13:27
-- Generated from EDMX file: C:\Users\Mariano\source\repos\yumiko\YumikoBot\Data Access Layer\modelo.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Yumiko];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[LeaderboardPersonajes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LeaderboardPersonajes];
GO
IF OBJECT_ID(N'[dbo].[LeaderboardAnimes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LeaderboardAnimes];
GO
IF OBJECT_ID(N'[dbo].[UsuariosDiscord]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UsuariosDiscord];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'LeaderboardPersonajes'
CREATE TABLE [dbo].[LeaderboardPersonajes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [user_id] bigint  NOT NULL,
    [guild_id] bigint  NOT NULL,
    [dificultad] nvarchar(max)  NOT NULL,
    [partidasJugadas] int  NOT NULL,
    [rondasAcertadas] int  NOT NULL,
    [rondasTotales] int  NOT NULL
);
GO

-- Creating table 'LeaderboardAnimes'
CREATE TABLE [dbo].[LeaderboardAnimes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [user_id] bigint  NOT NULL,
    [guild_id] bigint  NOT NULL,
    [dificultad] nvarchar(max)  NOT NULL,
    [partidasJugadas] int  NOT NULL,
    [rondasAcertadas] int  NOT NULL,
    [rondasTotales] int  NOT NULL
);
GO

-- Creating table 'UsuariosDiscord'
CREATE TABLE [dbo].[UsuariosDiscord] (
    [Id] bigint  NOT NULL,
    [guild_id] bigint  NOT NULL,
    [Birthday] datetime  NOT NULL,
    [MostrarYear] bit  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'LeaderboardPersonajes'
ALTER TABLE [dbo].[LeaderboardPersonajes]
ADD CONSTRAINT [PK_LeaderboardPersonajes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'LeaderboardAnimes'
ALTER TABLE [dbo].[LeaderboardAnimes]
ADD CONSTRAINT [PK_LeaderboardAnimes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UsuariosDiscord'
ALTER TABLE [dbo].[UsuariosDiscord]
ADD CONSTRAINT [PK_UsuariosDiscord]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------