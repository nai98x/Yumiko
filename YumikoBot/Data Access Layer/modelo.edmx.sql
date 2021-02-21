
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 01/30/2021 15:42:09
-- Generated from EDMX file: C:\Users\corsa\source\repos\nai98x\yumiko\YumikoBot\Data Access Layer\modelo.edmx
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

IF OBJECT_ID(N'[dbo].[UsuariosDiscord]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UsuariosDiscord];
GO
IF OBJECT_ID(N'[dbo].[Imagenes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Imagenes];
GO
IF OBJECT_ID(N'[dbo].[LeaderboardSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LeaderboardSet];
GO
IF OBJECT_ID(N'[dbo].[CanalAnunciosSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CanalAnunciosSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'UsuariosDiscord'
CREATE TABLE [dbo].[UsuariosDiscord] (
    [Id] bigint  NOT NULL,
    [guild_id] bigint  NOT NULL,
    [Birthday] datetime  NOT NULL,
    [MostrarYear] bit  NULL,
    [Anilist] nvarchar(max)  NULL
);
GO

-- Creating table 'Imagenes'
CREATE TABLE [dbo].[Imagenes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] nvarchar(max)  NOT NULL,
    [Comando] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'LeaderboardSet'
CREATE TABLE [dbo].[LeaderboardSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [user_id] bigint  NOT NULL,
    [guild_id] bigint  NOT NULL,
    [juego] nvarchar(max)  NOT NULL,
    [dificultad] nvarchar(max)  NOT NULL,
    [partidasJugadas] int  NOT NULL,
    [rondasAcertadas] int  NOT NULL,
    [rondasTotales] int  NOT NULL
);
GO

-- Creating table 'CanalAnunciosSet'
CREATE TABLE [dbo].[CanalAnunciosSet] (
    [guild_id] bigint  NOT NULL,
    [channel_id] bigint  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id], [guild_id] in table 'UsuariosDiscord'
ALTER TABLE [dbo].[UsuariosDiscord]
ADD CONSTRAINT [PK_UsuariosDiscord]
    PRIMARY KEY CLUSTERED ([Id], [guild_id] ASC);
GO

-- Creating primary key on [Id] in table 'Imagenes'
ALTER TABLE [dbo].[Imagenes]
ADD CONSTRAINT [PK_Imagenes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'LeaderboardSet'
ALTER TABLE [dbo].[LeaderboardSet]
ADD CONSTRAINT [PK_LeaderboardSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [guild_id] in table 'CanalAnunciosSet'
ALTER TABLE [dbo].[CanalAnunciosSet]
ADD CONSTRAINT [PK_CanalAnunciosSet]
    PRIMARY KEY CLUSTERED ([guild_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------