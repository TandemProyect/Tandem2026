Orden de ejecución (TemporalScript — Cliente / Obra V2)

======================================================



Base de datos: misma instancia que Desing (dbo.AspNetUsers debe existir).



1) 2026-05-15_create_TSql_Client_V2.sql

   Crea dbo.TSql_Client_V2 (instalación nueva).



2a) 2026-05-15_create_TSql_Jobside.sql

    Crea dbo.TSql_Jobside con LinkClient_V2 (instalación nueva).

    Ejecutar SOLO si la tabla Jobside NO existe aún.



2b) 2026-05-15_alter_TSql_Jobside_LinkClient_V2.sql

    Migra Jobside existente: Link_Client → LinkClient_V2 + FK.

    Ejecutar en lugar de 2a si dbo.TSql_Jobside ya existía.



3) 2026-05-15_align_TSql_Client_V2_Jobside_audit_security.sql

   Alinea columnas de auditoría intranet y FK a dbo.AspNetUsers

   (LinkMadeBy, LinModifiedBy, AddChangeBy). Idempotente.

   Ejecutar SIEMPRE después de 1 y (2a o 2b).



Notas

-----

- Diagram_General no está en el repositorio; convenciones tomadas de DAL/Model.edmx.

- Tablas legacy del EDMX usan BitIsDeleted; intranet Client/Jobside usan Is_Delete.

- En INSERT: rellenar LinkMadeBy, LinModifiedBy, AddChangeBy con UserId (AspNet),

  AddDateMade y AddLastDateChange con GETDATE(), Ntimeschanged = 0.

- En UPDATE: LinModifiedBy, AddChangeBy, AddLastDateChange, incrementar Ntimeschanged.



Referencia alternativa al diagrama: DAL/Model.edmx.diagram (nombre interno "Diagram1").

