-- Wait for SQL Server to be ready, then run setup
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PokerTrack')
BEGIN
    CREATE DATABASE PokerTrack;
END
GO

USE PokerTrack;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    CREATE TABLE Sessions (
        Id                INT            IDENTITY(1,1) PRIMARY KEY,
        UserId            NVARCHAR(128)  NOT NULL,
        SessionDate       DATE           NOT NULL,
        VenueName         NVARCHAR(100)  NOT NULL,
        GameType          NVARCHAR(20)   NOT NULL DEFAULT 'Cash',
        StakesDescription NVARCHAR(20)   NOT NULL DEFAULT '',
        BuyInAmount       DECIMAL(10,2)  NOT NULL,
        CashOutAmount     DECIMAL(10,2)  NOT NULL,
        DurationMinutes   INT            NOT NULL,
        Notes             NVARCHAR(1000) NULL,
        CreatedAt         DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt         DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Analytics')
BEGIN
    CREATE TABLE Analytics (
        UserId              NVARCHAR(128)  NOT NULL PRIMARY KEY,
        TotalSessions       INT            NOT NULL DEFAULT 0,
        TotalProfit         DECIMAL(12,2)  NOT NULL DEFAULT 0,
        AvgProfitPerSession DECIMAL(10,2)  NOT NULL DEFAULT 0,
        AvgHourlyRate       DECIMAL(10,2)  NOT NULL DEFAULT 0,
        WinningSessions     INT            NOT NULL DEFAULT 0,
        LosingSessions      INT            NOT NULL DEFAULT 0,
        WinRate             DECIMAL(5,4)   NOT NULL DEFAULT 0,
        CurrentStreak       INT            NOT NULL DEFAULT 0,
        LongestWinStreak    INT            NOT NULL DEFAULT 0,
        BiggestWin          DECIMAL(10,2)  NOT NULL DEFAULT 0,
        BiggestLoss         DECIMAL(10,2)  NOT NULL DEFAULT 0,
        LastUpdated         DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sessions_UserId_SessionDate')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Sessions_UserId_SessionDate
        ON Sessions (UserId, SessionDate DESC);
END
GO

-- Enable CDC if not already enabled
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'PokerTrack' AND is_cdc_enabled = 1)
BEGIN
    EXEC sys.sp_cdc_enable_db;
END
GO

IF NOT EXISTS (SELECT 1 FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('dbo.Sessions'))
BEGIN
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'dbo',
        @source_name   = N'Sessions',
        @role_name     = NULL;
END
GO