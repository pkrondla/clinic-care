-- Idempotent default subscription plans (extend as needed)
IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlans] WHERE [Name] = N'Trial')
BEGIN
    INSERT INTO [dbo].[SubscriptionPlans] ([Name], [Description], [Price], [DurationDays], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (N'Trial', N'30-day trial access', 0, 30, 1, GETUTCDATE(), GETUTCDATE());
END
GO
