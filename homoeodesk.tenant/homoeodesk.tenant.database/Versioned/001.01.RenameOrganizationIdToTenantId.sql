-- Versioned migration: rename OrganizationId column to TenantId where present
-- Safe to run multiple times (checks sys.columns)

DECLARE @sql NVARCHAR(MAX);

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
SELECT 'IF COL_LENGTH(''' + SCHEMA_NAME(t.schema_id) + '.' + t.name + ''', ''OrganizationId'') IS NOT NULL
    AND COL_LENGTH(''' + SCHEMA_NAME(t.schema_id) + '.' + t.name + ''', ''TenantId'') IS NULL
BEGIN
    EXEC sp_rename ''' + SCHEMA_NAME(t.schema_id) + '.' + t.name + '.OrganizationId'', ''TenantId'', ''COLUMN'';
END'
FROM sys.tables t
WHERE t.name NOT IN ('Organizations')
  AND EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = t.object_id AND c.name = 'OrganizationId'
  );

OPEN cur;
FETCH NEXT FROM cur INTO @sql;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_executesql @sql;
    FETCH NEXT FROM cur INTO @sql;
END
CLOSE cur;
DEALLOCATE cur;
GO
