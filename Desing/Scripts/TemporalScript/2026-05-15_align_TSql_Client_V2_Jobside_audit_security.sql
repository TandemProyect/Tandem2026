/* =============================================================================
   Script 4 — Alineación auditoría y seguridad (AspNetUsers)
   Tablas: dbo.TSql_Client_V2, dbo.TSql_Jobside

   Referencia (Diagram_General no localizado en repo):
   - DAL/Model.edmx (SSDL) + diagrama EF "Diagram1" en Model.edmx.diagram
   - Patrón: LinkMadeBy, LinModifiedBy, AddChangeBy → dbo.AspNetUsers(Id)
   - Fechas: AddDateMade, AddLastDateChange; contador Ntimeschanged

   Requiere Scripts 1–3. Idempotente (COL_LENGTH / sys.foreign_keys).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* =============================================================================
   SECCIÓN A — dbo.TSql_Client_V2
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
        OR NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers)
    BEGIN
        RAISERROR(N'Script 4: dbo.AspNetUsers debe existir y tener al menos un usuario.', 16, 1);
    END
    ELSE
    BEGIN
        DECLARE @FallbackUserId NVARCHAR(128);
        SELECT TOP (1) @FallbackUserId = Id FROM dbo.AspNetUsers ORDER BY Id;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'TextLabel') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD TextLabel NVARCHAR(500) NOT NULL
                CONSTRAINT DF_TSql_Client_V2_TextLabel DEFAULT (N'');

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Is_Delete') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD Is_Delete BIT NOT NULL
                CONSTRAINT DF_TSql_Client_V2_Is_Delete DEFAULT (0);

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Is_Active') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD Is_Active BIT NOT NULL
                CONSTRAINT DF_TSql_Client_V2_Is_Active DEFAULT (1);

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'LinkMadeBy') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD LinkMadeBy NVARCHAR(128) NULL;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'LinModifiedBy') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD LinModifiedBy NVARCHAR(128) NULL;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'AddDateMade') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD AddDateMade DATETIME NULL;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'AddChangeBy') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD AddChangeBy NVARCHAR(128) NULL;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'AddLastDateChange') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD AddLastDateChange DATETIME NULL;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Ntimeschanged') IS NULL
            ALTER TABLE dbo.TSql_Client_V2 ADD Ntimeschanged BIGINT NULL;

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'BitIsDeleted') IS NOT NULL
           AND COL_LENGTH(N'dbo.TSql_Client_V2', N'Is_Delete') IS NULL
            EXEC sp_rename N'dbo.TSql_Client_V2.BitIsDeleted', N'Is_Delete', N'COLUMN';

        IF COL_LENGTH(N'dbo.TSql_Client_V2', N'LinkChangeBy') IS NOT NULL
           AND COL_LENGTH(N'dbo.TSql_Client_V2', N'AddChangeBy') IS NULL
            EXEC sp_rename N'dbo.TSql_Client_V2.LinkChangeBy', N'AddChangeBy', N'COLUMN';

        UPDATE c
           SET LinkMadeBy = COALESCE(NULLIF(LTRIM(RTRIM(c.LinkMadeBy)), N''), @FallbackUserId),
               LinModifiedBy = COALESCE(NULLIF(LTRIM(RTRIM(c.LinModifiedBy)), N''), @FallbackUserId),
               AddChangeBy = COALESCE(NULLIF(LTRIM(RTRIM(c.AddChangeBy)), N''), @FallbackUserId),
               AddDateMade = COALESCE(c.AddDateMade, GETDATE()),
               AddLastDateChange = COALESCE(c.AddLastDateChange, GETDATE()),
               Ntimeschanged = COALESCE(c.Ntimeschanged, 0)
          FROM dbo.TSql_Client_V2 c
         WHERE c.LinkMadeBy IS NULL OR c.LinModifiedBy IS NULL OR c.AddChangeBy IS NULL
            OR c.AddDateMade IS NULL OR c.AddLastDateChange IS NULL OR c.Ntimeschanged IS NULL;

        UPDATE c SET LinkMadeBy = @FallbackUserId
          FROM dbo.TSql_Client_V2 c
         WHERE c.LinkMadeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        UPDATE c SET LinModifiedBy = @FallbackUserId
          FROM dbo.TSql_Client_V2 c
         WHERE c.LinModifiedBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        UPDATE c SET AddChangeBy = @FallbackUserId
          FROM dbo.TSql_Client_V2 c
         WHERE c.AddChangeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'LinkMadeBy' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN LinkMadeBy NVARCHAR(128) NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'LinModifiedBy' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN LinModifiedBy NVARCHAR(128) NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'AddChangeBy' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN AddChangeBy NVARCHAR(128) NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'AddDateMade' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN AddDateMade DATETIME NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'AddLastDateChange' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN AddLastDateChange DATETIME NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'Ntimeschanged' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN Ntimeschanged BIGINT NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'DF_TSql_Client_V2_AddDateMade')
            ALTER TABLE dbo.TSql_Client_V2 ADD CONSTRAINT DF_TSql_Client_V2_AddDateMade DEFAULT (GETDATE()) FOR AddDateMade;

        IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'DF_TSql_Client_V2_AddLastDateChange')
            ALTER TABLE dbo.TSql_Client_V2 ADD CONSTRAINT DF_TSql_Client_V2_AddLastDateChange DEFAULT (GETDATE()) FOR AddLastDateChange;

        IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2') AND name = N'DF_TSql_Client_V2_Ntimeschanged')
            ALTER TABLE dbo.TSql_Client_V2 ADD CONSTRAINT DF_TSql_Client_V2_Ntimeschanged DEFAULT (0) FOR Ntimeschanged;

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Client_V2_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2'))
            ALTER TABLE dbo.TSql_Client_V2 ADD CONSTRAINT FK_TSql_Client_V2_AspNetUsers
                FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Client_V2_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2'))
            ALTER TABLE dbo.TSql_Client_V2 ADD CONSTRAINT FK_TSql_Client_V2_AspNetUsers1
                FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Client_V2_AspNetUsers2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2'))
            ALTER TABLE dbo.TSql_Client_V2 ADD CONSTRAINT FK_TSql_Client_V2_AspNetUsers2
                FOREIGN KEY (AddChangeBy) REFERENCES dbo.AspNetUsers (Id);
    END
END
ELSE
    PRINT N'Aviso: dbo.TSql_Client_V2 no existe; ejecute Script 1.';
GO

/* =============================================================================
   SECCIÓN B — dbo.TSql_Jobside
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
        OR NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers)
    BEGIN
        RAISERROR(N'Script 4: dbo.AspNetUsers debe existir y tener al menos un usuario.', 16, 1);
    END
    ELSE
    BEGIN
        DECLARE @FallbackUserIdJob NVARCHAR(128);
        SELECT TOP (1) @FallbackUserIdJob = Id FROM dbo.AspNetUsers ORDER BY Id;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'TextLabel') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD TextLabel NVARCHAR(500) NOT NULL
                CONSTRAINT DF_TSql_Jobside_TextLabel DEFAULT (N'');

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'Is_Delete') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD Is_Delete BIT NOT NULL
                CONSTRAINT DF_TSql_Jobside_Is_Delete DEFAULT (0);

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'Is_Active') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD Is_Active BIT NOT NULL
                CONSTRAINT DF_TSql_Jobside_Is_Active DEFAULT (1);

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'LinkMadeBy') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD LinkMadeBy NVARCHAR(128) NULL;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'LinModifiedBy') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD LinModifiedBy NVARCHAR(128) NULL;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddDateMade') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD AddDateMade DATETIME NULL;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddChangeBy') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD AddChangeBy NVARCHAR(128) NULL;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddLastDateChange') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD AddLastDateChange DATETIME NULL;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'Ntimeschanged') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD Ntimeschanged BIGINT NULL;

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'BitIsDeleted') IS NOT NULL
           AND COL_LENGTH(N'dbo.TSql_Jobside', N'Is_Delete') IS NULL
            EXEC sp_rename N'dbo.TSql_Jobside.BitIsDeleted', N'Is_Delete', N'COLUMN';

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'LinkChangeBy') IS NOT NULL
           AND COL_LENGTH(N'dbo.TSql_Jobside', N'AddChangeBy') IS NULL
            EXEC sp_rename N'dbo.TSql_Jobside.LinkChangeBy', N'AddChangeBy', N'COLUMN';

        UPDATE j
           SET LinkMadeBy = COALESCE(NULLIF(LTRIM(RTRIM(j.LinkMadeBy)), N''), @FallbackUserIdJob),
               LinModifiedBy = COALESCE(NULLIF(LTRIM(RTRIM(j.LinModifiedBy)), N''), @FallbackUserIdJob),
               AddChangeBy = COALESCE(NULLIF(LTRIM(RTRIM(j.AddChangeBy)), N''), @FallbackUserIdJob),
               AddDateMade = COALESCE(j.AddDateMade, GETDATE()),
               AddLastDateChange = COALESCE(j.AddLastDateChange, GETDATE()),
               Ntimeschanged = COALESCE(j.Ntimeschanged, 0)
          FROM dbo.TSql_Jobside j
         WHERE j.LinkMadeBy IS NULL OR j.LinModifiedBy IS NULL OR j.AddChangeBy IS NULL
            OR j.AddDateMade IS NULL OR j.AddLastDateChange IS NULL OR j.Ntimeschanged IS NULL;

        UPDATE j SET LinkMadeBy = @FallbackUserIdJob
          FROM dbo.TSql_Jobside j
         WHERE j.LinkMadeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        UPDATE j SET LinModifiedBy = @FallbackUserIdJob
          FROM dbo.TSql_Jobside j
         WHERE j.LinModifiedBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        UPDATE j SET AddChangeBy = @FallbackUserIdJob
          FROM dbo.TSql_Jobside j
         WHERE j.AddChangeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'LinkMadeBy' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN LinkMadeBy NVARCHAR(128) NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'LinModifiedBy' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN LinModifiedBy NVARCHAR(128) NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'AddChangeBy' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN AddChangeBy NVARCHAR(128) NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'AddDateMade' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN AddDateMade DATETIME NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'AddLastDateChange' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN AddLastDateChange DATETIME NOT NULL;

        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'Ntimeschanged' AND is_nullable = 1)
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN Ntimeschanged BIGINT NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'DF_TSql_Jobside_AddDateMade')
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT DF_TSql_Jobside_AddDateMade DEFAULT (GETDATE()) FOR AddDateMade;

        IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'DF_TSql_Jobside_AddLastDateChange')
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT DF_TSql_Jobside_AddLastDateChange DEFAULT (GETDATE()) FOR AddLastDateChange;

        IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside') AND name = N'DF_TSql_Jobside_Ntimeschanged')
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT DF_TSql_Jobside_Ntimeschanged DEFAULT (0) FOR Ntimeschanged;

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Jobside_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside'))
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT FK_TSql_Jobside_AspNetUsers
                FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Jobside_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside'))
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT FK_TSql_Jobside_AspNetUsers1
                FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Jobside_AspNetUsers2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside'))
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT FK_TSql_Jobside_AspNetUsers2
                FOREIGN KEY (AddChangeBy) REFERENCES dbo.AspNetUsers (Id);

        IF COL_LENGTH(N'dbo.TSql_Jobside', N'LinkClient_V2') IS NULL
            ALTER TABLE dbo.TSql_Jobside ADD LinkClient_V2 BIGINT NULL;

        IF OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Jobside_TSql_Client_V2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside'))
            ALTER TABLE dbo.TSql_Jobside ADD CONSTRAINT FK_TSql_Jobside_TSql_Client_V2
                FOREIGN KEY (LinkClient_V2) REFERENCES dbo.TSql_Client_V2 (IdObject);
    END
END
ELSE
    PRINT N'Aviso: dbo.TSql_Jobside no existe; ejecute Script 2 o 3.';
GO

PRINT N'OK — Script 4: auditoría y FK AspNetUsers en TSql_Client_V2 / TSql_Jobside.';
GO
