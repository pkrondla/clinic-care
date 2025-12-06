-- ====================================
-- ClinicCare Tenant Database - PurchaseOrders and PurchaseOrderItems Tables
-- ====================================
-- These tables store purchase order information for inventory management
-- ====================================

USE ClinicCare_demo; -- Replace with actual tenant database name
GO

-- Create PurchaseOrders table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PurchaseOrders')
BEGIN
    CREATE TABLE PurchaseOrders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        ClinicId INT NOT NULL,
        SupplierId INT NOT NULL,
        OrderNumber NVARCHAR(50) NOT NULL,
        OrderDate DATETIME2 NOT NULL,
        ExpectedDeliveryDate DATETIME2 NULL,
        Status INT NOT NULL DEFAULT 1, -- 1=Draft, 2=Pending, 3=Approved, 4=Ordered, 5=PartiallyReceived, 6=Received, 7=Cancelled
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NULL,
        TaxAmount DECIMAL(18,2) NULL,
        GrandTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        Notes NVARCHAR(1000) NULL,
        ApprovedDate DATETIME2 NULL,
        ApprovedByUserId INT NULL,
        OrderedDate DATETIME2 NULL,
        ReceivedDate DATETIME2 NULL,
        ReceivedByUserId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_PurchaseOrders_Clinics FOREIGN KEY (ClinicId) 
            REFERENCES Clinics(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_PurchaseOrders_Suppliers FOREIGN KEY (SupplierId) 
            REFERENCES Suppliers(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_PurchaseOrders_ApprovedByUser FOREIGN KEY (ApprovedByUserId) 
            REFERENCES Users(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_PurchaseOrders_ReceivedByUser FOREIGN KEY (ReceivedByUserId) 
            REFERENCES Users(Id) ON DELETE NO ACTION
    );
    
    CREATE UNIQUE INDEX IX_PurchaseOrders_OrganizationOrderNumber 
        ON PurchaseOrders(OrganizationId, OrderNumber);
    CREATE INDEX IX_PurchaseOrders_ClinicDate 
        ON PurchaseOrders(OrganizationId, ClinicId, OrderDate);
    
    PRINT 'Table PurchaseOrders created.';
END
ELSE
BEGIN
    PRINT 'Table PurchaseOrders already exists.';
END
GO

-- Create PurchaseOrderItems table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PurchaseOrderItems')
BEGIN
    CREATE TABLE PurchaseOrderItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        PurchaseOrderId INT NOT NULL,
        MedicineId INT NOT NULL,
        Quantity INT NOT NULL,
        ReceivedQuantity INT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        BatchNumber NVARCHAR(50) NULL,
        ExpiryDate DATE NULL,
        Notes NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_PurchaseOrderItems_PurchaseOrders FOREIGN KEY (PurchaseOrderId) 
            REFERENCES PurchaseOrders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PurchaseOrderItems_ClinicMedicines FOREIGN KEY (MedicineId) 
            REFERENCES ClinicMedicines(Id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_PurchaseOrderItems_PurchaseOrder 
        ON PurchaseOrderItems(OrganizationId, PurchaseOrderId);
    
    PRINT 'Table PurchaseOrderItems created.';
END
ELSE
BEGIN
    PRINT 'Table PurchaseOrderItems already exists.';
END
GO

