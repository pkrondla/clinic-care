-- Idempotent default subscription plans aligned with SubscriptionPlans schema
IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlans] WHERE [Name] = N'Trial')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans]
        ([Name], [Description], [Price], [BillingCycle], [MaxClinics], [MaxDoctors], [MaxPatients], [Features], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES
        (N'Trial', N'30-day trial access', 0, 1, 1, 3, 500, N'["appointments","queue","consultations","prescriptions","inventory","billing"]', 1, GETUTCDATE(), GETUTCDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlans] WHERE [Name] = N'Clinic')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans]
        ([Name], [Description], [Price], [BillingCycle], [MaxClinics], [MaxDoctors], [MaxPatients], [Features], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES
        (N'Clinic', N'Single-clinic production plan', 0, 1, 1, 10, 5000, N'["appointments","queue","consultations","prescriptions","inventory","billing","notifications","reports"]', 1, GETUTCDATE(), GETUTCDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlans] WHERE [Name] = N'Practice+')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans]
        ([Name], [Description], [Price], [BillingCycle], [MaxClinics], [MaxDoctors], [MaxPatients], [Features], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES
        (N'Practice+', N'Multi-branch practice plan', 0, 1, 5, 50, 50000, N'["appointments","queue","consultations","prescriptions","inventory","billing","notifications","reports","multi-branch","priority-support"]', 1, GETUTCDATE(), GETUTCDATE());
END
GO
