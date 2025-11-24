-- ====================================
-- ClinicCare Global Database - Payment Transactions Table
-- ====================================
-- This table stores payment history and billing
-- ====================================

USE ClinicCare_Global;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentTransactions')
BEGIN
    CREATE TABLE PaymentTransactions (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        OrganizationId INT NOT NULL,
        SubscriptionId INT NOT NULL,
        Amount DECIMAL(10,2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL DEFAULT 'INR',
        PaymentMethod NVARCHAR(50) NOT NULL, -- Card, UPI, NetBanking, etc.
        PaymentGateway NVARCHAR(50) NOT NULL,
        TransactionId NVARCHAR(200) NOT NULL,
        Status INT NOT NULL DEFAULT 1, -- 1=Pending, 2=Success, 3=Failed, 4=Refunded
        PaymentDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_PaymentTransactions_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
        CONSTRAINT FK_PaymentTransactions_Subscription FOREIGN KEY (SubscriptionId) REFERENCES OrganizationSubscriptions(Id),
        CONSTRAINT UK_PaymentTransactions_TransactionId UNIQUE (TransactionId)
    );
    PRINT 'Table PaymentTransactions created.';
END
ELSE
BEGIN
    PRINT 'Table PaymentTransactions already exists.';
END
GO

