DECLARE @TenantId INT = $(TenantId);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Organizations] WHERE [Id] = @TenantId)
BEGIN
    SET IDENTITY_INSERT [dbo].[Organizations] ON;
    INSERT INTO [dbo].[Organizations] ([Id], [Name], [Subdomain], [ContactEmail], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (@TenantId, N'Default Tenant', N'demo', N'admin@homoeodesk.com', 1, GETUTCDATE(), GETUTCDATE());
    SET IDENTITY_INSERT [dbo].[Organizations] OFF;
END
GO
