IF OBJECT_ID('dbo.TSql_TelegramDesignInbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_TelegramDesignInbox
    (
        SysObjectID       BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LinDesign         BIGINT NOT NULL,
        TelegramMessageId NVARCHAR(64) NULL,
        TelegramChatId    NVARCHAR(64) NULL,
        TelegramUserId    NVARCHAR(64) NULL,
        TelegramUserName  NVARCHAR(120) NULL,
        Caption           NVARCHAR(1000) NULL,
        FileId            NVARCHAR(255) NOT NULL,
        FileUniqueId      NVARCHAR(255) NULL,
        WidthPx           INT NULL,
        HeightPx          INT NULL,
        Estado            NVARCHAR(40) NOT NULL CONSTRAINT DF_TSql_TelegramDesignInbox_Estado DEFAULT('Pendiente'),
        FechaMensajeUtc   DATETIME2(0) NOT NULL,
        FechaRegistroUtc  DATETIME2(0) NOT NULL CONSTRAINT DF_TSql_TelegramDesignInbox_FechaRegistro DEFAULT(SYSUTCDATETIME())
    );
END
GO

IF OBJECT_ID('dbo.TSql_TelegramDesignAccess', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_TelegramDesignAccess
    (
        SysObjectID      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LinDesign        BIGINT NOT NULL,
        TelegramChatId   NVARCHAR(64) NULL,
        TelegramUserId   NVARCHAR(64) NULL,
        Alias            NVARCHAR(150) NULL,
        IsActive         BIT NOT NULL CONSTRAINT DF_TSql_TelegramDesignAccess_IsActive DEFAULT(1),
        FechaRegistroUtc DATETIME2(0) NOT NULL CONSTRAINT DF_TSql_TelegramDesignAccess_Fecha DEFAULT(SYSUTCDATETIME())
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TSql_TelegramDesignInbox_TSql_Design'
      AND parent_object_id = OBJECT_ID('dbo.TSql_TelegramDesignInbox')
)
BEGIN
    ALTER TABLE dbo.TSql_TelegramDesignInbox
        ADD CONSTRAINT FK_TSql_TelegramDesignInbox_TSql_Design
            FOREIGN KEY (LinDesign) REFERENCES dbo.TSql_Design(SysObjectID);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TSql_TelegramDesignAccess_TSql_Design'
      AND parent_object_id = OBJECT_ID('dbo.TSql_TelegramDesignAccess')
)
BEGIN
    ALTER TABLE dbo.TSql_TelegramDesignAccess
        ADD CONSTRAINT FK_TSql_TelegramDesignAccess_TSql_Design
            FOREIGN KEY (LinDesign) REFERENCES dbo.TSql_Design(SysObjectID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TSql_TelegramDesignInbox_LinDesign_Estado' AND object_id = OBJECT_ID('dbo.TSql_TelegramDesignInbox'))
BEGIN
    CREATE INDEX IX_TSql_TelegramDesignInbox_LinDesign_Estado
        ON dbo.TSql_TelegramDesignInbox(LinDesign, Estado, FechaRegistroUtc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TSql_TelegramDesignAccess_LinDesign_Active' AND object_id = OBJECT_ID('dbo.TSql_TelegramDesignAccess'))
BEGIN
    CREATE INDEX IX_TSql_TelegramDesignAccess_LinDesign_Active
        ON dbo.TSql_TelegramDesignAccess(LinDesign, IsActive, TelegramChatId, TelegramUserId);
END
GO

-- Ejemplo: habilitar un chat o usuario para el diseno 131.
-- Reemplaza valores por los reales y ejecuta:
-- INSERT INTO dbo.TSql_TelegramDesignAccess (LinDesign, TelegramChatId, TelegramUserId, Alias, IsActive)
-- VALUES (131, '-1001234567890', NULL, 'Grupo Obra 131', 1);
--
-- INSERT INTO dbo.TSql_TelegramDesignAccess (LinDesign, TelegramChatId, TelegramUserId, Alias, IsActive)
-- VALUES (131, NULL, '123456789', 'Usuario Campo Juan', 1);
