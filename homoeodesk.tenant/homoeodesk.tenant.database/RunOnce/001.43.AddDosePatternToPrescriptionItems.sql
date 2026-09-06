-- ====================================
-- Add DosePattern to PrescriptionItems
-- ====================================
-- Stores M/A/N notation such as "4-0-4" for label printing
-- ====================================

USE HomoeoDesk_demo;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PrescriptionItems')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PrescriptionItems') AND name = 'DosePattern')
BEGIN
    ALTER TABLE PrescriptionItems ADD DosePattern NVARCHAR(50) NULL;
    PRINT 'Added DosePattern column to PrescriptionItems.';
END
ELSE
BEGIN
    PRINT 'DosePattern already exists or PrescriptionItems table missing — skipped.';
END
GO
