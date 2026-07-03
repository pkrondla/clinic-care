-- ====================================
-- HomoeoDesk Tenant Database - Invoice Items Table
-- ====================================
-- This table stores invoice line items
-- Replace {TENANT_ID} with actual tenant identifier
-- ====================================

USE HomoeoDesk_{TENANT_ID};
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceItems')
BEGIN
    CREATE TABLE InvoiceItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        InvoiceId INT NOT NULL,
        ItemType NVARCHAR(50) NOT NULL, -- Consultation, Medicine, Courier, etc.
        Description NVARCHAR(200) NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(10,2) NOT NULL,
        TotalPrice DECIMAL(10,2) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_InvoiceItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
    );
    PRINT 'Table InvoiceItems created.';
END
ELSE
BEGIN
    PRINT 'Table InvoiceItems already exists.';
END
GO


