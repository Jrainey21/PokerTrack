-- PokerTrack schema
-- Run this against your SQL Server instance (or let Docker init run it on first start).

CREATE DATABASE PokerTrack;
GO

USE PokerTrack;
GO

CREATE TABLE Sessions (
    Id                INT            IDENTITY(1,1) PRIMARY KEY,
    UserId            NVARCHAR(128)  NOT NULL,         -- Entra ID object ID
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
GO

-- Analytics are written by the Worker; the web app only reads this table.
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
GO

-- Index so session queries by user stay fast as data grows.
CREATE NONCLUSTERED INDEX IX_Sessions_UserId_SessionDate
    ON Sessions (UserId, SessionDate DESC);
GO

-- CDC must be enabled for Debezium to capture changes.
-- Run this AFTER the database and tables are created.
EXEC sys.sp_cdc_enable_db;
GO

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name   = N'Sessions',
    @role_name     = NULL;
GO
