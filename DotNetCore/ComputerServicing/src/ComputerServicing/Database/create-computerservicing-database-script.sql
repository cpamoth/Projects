CREATE DATABASE [ComputerServices];
GO

USE [ComputerServices];
GO

CREATE TABLE [Users] (
    [UsersId] int NOT NULL IDENTITY,
    [Url] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UsersId])
);
GO

INSERT INTO [Users] (Url) VALUES
('administrator'),
('testuser1'),
('testuser2')
GO

CREATE TABLE [Services] (
    [ServiceId] NVARCHAR NOT NULL IDENTITY,
    [URL] NVARCHAR(max) NOT NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId])
)
GO

INSERT INTO [Services] (Url) VALUES
('Virus Removal'),
('Company Network Lab Installation Setup'),
('Residential Computer and Network Installation and Troubleshooting'),
('Web Site Design')
GO