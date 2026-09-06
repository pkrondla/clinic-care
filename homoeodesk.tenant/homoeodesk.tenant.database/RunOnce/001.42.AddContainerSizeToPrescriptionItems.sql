-- ====================================
-- Align PrescriptionItems with PrescriptionItem entity
-- ====================================
-- Adds dispensing fields used by the prescription UI/API
-- ====================================

USE HomoeoDesk_demo;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PrescriptionItems')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PrescriptionItems') AND name = 'ContainerSize')
BEGIN
    ALTER TABLE PrescriptionItems ADD ContainerSize INT NULL;
    PRINT 'Added ContainerSize column to PrescriptionItems.';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PrescriptionItems')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PrescriptionItems') AND name = 'DispensingForm')
BEGIN
    ALTER TABLE PrescriptionItems ADD DispensingForm INT NOT NULL CONSTRAINT DF_PrescriptionItems_DispensingForm DEFAULT 1;
    PRINT 'Added DispensingForm column to PrescriptionItems.';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PrescriptionItems')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PrescriptionItems') AND name = 'Timing')
BEGIN
    ALTER TABLE PrescriptionItems ADD Timing NVARCHAR(100) NOT NULL CONSTRAINT DF_PrescriptionItems_Timing DEFAULT '';
    PRINT 'Added Timing column to PrescriptionItems.';
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PrescriptionItems')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PrescriptionItems') AND name = 'DispensedQuantity')
BEGIN
    ALTER TABLE PrescriptionItems ADD DispensedQuantity DECIMAL(18,2) NOT NULL CONSTRAINT DF_PrescriptionItems_DispensedQuantity DEFAULT 0;
    PRINT 'Added DispensedQuantity column to PrescriptionItems.';
END
GO
