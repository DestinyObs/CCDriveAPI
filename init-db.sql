-- TheDrive API Database Initialization Script
-- This script creates the main database for the TheDrive API

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TheDriveAPI')
BEGIN
    CREATE DATABASE [TheDriveAPI];
    PRINT 'Database TheDriveAPI created successfully';
END
ELSE
BEGIN
    PRINT 'Database TheDriveAPI already exists';
END
GO

-- Switch to the TheDriveAPI database
USE [TheDriveAPI];
GO

-- Grant permissions to SA user (already has sysadmin but being explicit)
-- The Entity Framework migrations will create all tables automatically
PRINT 'Database initialization completed. EF migrations will create all tables.';
GO
