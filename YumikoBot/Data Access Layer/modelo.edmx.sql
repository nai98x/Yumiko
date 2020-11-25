
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/25/2020 19:04:23
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

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'LeaderboardPersonajes'
CREATE TABLE [dbo].[LeaderboardPersonajes] (
    [user_id] int IDENTITY(1,1) NOT NULL,
    [guild_id] int  NOT NULL,
    [dificultad] nvarchar(max)  NOT NULL,
    [partidasJugadas] int  NOT NULL,
    [porcentajeAciertos] int  NOT NULL
);
GO

-- Creating table 'LeaderboardAnimes'
CREATE TABLE [dbo].[LeaderboardAnimes] (
    [user_id] int IDENTITY(1,1) NOT NULL,
    [guild_id] int  NOT NULL,
    [dificultad] nvarchar(max)  NOT NULL,
    [partidasJugadas] int  NOT NULL,
    [porcentajeAciertos] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [user_id], [guild_id] in table 'LeaderboardPersonajes'
ALTER TABLE [dbo].[LeaderboardPersonajes]
ADD CONSTRAINT [PK_LeaderboardPersonajes]
    PRIMARY KEY CLUSTERED ([user_id], [guild_id] ASC);
GO

-- Creating primary key on [user_id], [guild_id] in table 'LeaderboardAnimes'
ALTER TABLE [dbo].[LeaderboardAnimes]
ADD CONSTRAINT [PK_LeaderboardAnimes]
    PRIMARY KEY CLUSTERED ([user_id], [guild_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------