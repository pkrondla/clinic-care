-- Rename ClinicId columns to BranchId across tenant tables (idempotent)
-- Run after OrganizationId → TenantId migration

DECLARE @sql NVARCHAR(MAX);

DECLARE rename_cursor CURSOR FOR
SELECT
    'IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ''') AND name = N''ClinicId'')
     AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N''' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ''') AND name = N''BranchId'')
     EXEC sp_rename ''' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + '.ClinicId'', ''BranchId'', ''COLUMN'';'
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = 'ClinicId'
WHERE s.name = 'dbo';

OPEN rename_cursor;
FETCH NEXT FROM rename_cursor INTO @sql;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_executesql @sql;
    FETCH NEXT FROM rename_cursor INTO @sql;
END

CLOSE rename_cursor;
DEALLOCATE rename_cursor;
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Clinics' AND schema_id = SCHEMA_ID('dbo'))
   AND NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Branches' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    EXEC sp_rename 'dbo.Clinics', 'Branches';
    PRINT 'Renamed table Clinics to Branches';
END
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserClinicAccess' AND schema_id = SCHEMA_ID('dbo'))
   AND NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserBranchAccess' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    EXEC sp_rename 'dbo.UserClinicAccess', 'UserBranchAccess';
    PRINT 'Renamed table UserClinicAccess to UserBranchAccess';
END
GO
