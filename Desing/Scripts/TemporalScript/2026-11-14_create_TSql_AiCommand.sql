/* =============================================================================
   dbo.TSql_AiCommand — Catálogo global de comandos activables por la IA (diseño, muro y comunes),
   incluidas las herramientas del visor Desing_2 aún «pendiente de IA» (Is_AiImplemented = 0).

   TextCode: código estable (WALL_DRAW, MODE_FORMWORK, TOOL_ROTATE…), único con Is_Delete = 0.
   TextContext: design (IA del diseño) | wall (IA del muro) | common (sí/no/cancelar y asistente).
   TextHandler: qué ejecuta el visor —
       ai:<id>            comando del registro JS (design-ai-assistant.js / wall-ai-assistant.js)
       toolbar:<id>       maStlDesing2ActivateToolbarCommandById('<id>')
       click:<selector>   pulsar ese botón (como hace bridge.setMode)
       key:<tecla>        atajo de teclado
     Tras «|» se anota la función JS real como referencia.
   TextAiCommandId: id del registro JS (wall.draw, mode, materials…) o NULL.
   TextParamsJson: [{name, required, help}] (mismo formato que el registro JS).
   Is_Destructive / Is_Confirm: borra datos / pide confirmación antes de ejecutar.
   Convención: .cursor/rules/sql-tsql-table-conventions.mdc (9 columnas de auditoría, borrado lógico).
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiCommand
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiCommand PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        TextCode         NVARCHAR(60)  NOT NULL,
        TextContext      NVARCHAR(20)  NOT NULL
            CONSTRAINT CK_TSql_AiCommand_TextContext CHECK (TextContext IN (N'design', N'wall', N'common')),
        TextGroup        NVARCHAR(100) NOT NULL,
        TextNameEs       NVARCHAR(200) NOT NULL,
        TextNameEn       NVARCHAR(200) NOT NULL,
        TextDescription  NVARCHAR(500) NULL,
        TextHandler      NVARCHAR(300) NOT NULL,
        TextAiCommandId  NVARCHAR(60)  NULL,
        TextParamsJson   NVARCHAR(MAX) NULL,
        Is_Destructive   BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_Is_Destructive DEFAULT (0),
        Is_Confirm       BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_Is_Confirm DEFAULT (0),
        Is_AiImplemented BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_Is_AiImplemented DEFAULT (0),
        NumberOrder      INT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_NumberOrder DEFAULT (0),
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiCommand_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiCommand_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiCommand_TextCode_Active' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommand')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiCommand_TextCode_Active
        ON dbo.TSql_AiCommand (TextCode)
        WHERE Is_Delete = 0;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommand_Context_Order' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommand')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommand_Context_Order
        ON dbo.TSql_AiCommand (TextContext, NumberOrder)
        INCLUDE (TextCode, TextGroup, TextHandler, Is_Confirm, Is_AiImplemented)
        WHERE Is_Delete = 0;
GO
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommand_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommand'))
    ALTER TABLE dbo.TSql_AiCommand WITH CHECK ADD CONSTRAINT FK_TSql_AiCommand_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommand_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommand'))
    ALTER TABLE dbo.TSql_AiCommand WITH CHECK ADD CONSTRAINT FK_TSql_AiCommand_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiCommand creada (o ya existía).';
GO
