using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        const int defaultTenantId = 1;

        var hasUsers = await context.Users.AnyAsync();

        if (!hasUsers)
        {
            Console.WriteLine("DatabaseSeeder: No users found, creating default admin user...");

            var adminUser = new User
            {
                Email = "admin@healthcareplus.com",
                FirstName = "Admin",
                LastName = "User",
                Phone = "+1234567890",
                Role = UserRole.Admin,
                OrganizationId = defaultTenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = passwordHasher.HashPassword("Admin@123")
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            Console.WriteLine($"DatabaseSeeder: Created default admin user with ID: {adminUser.Id}");
        }
        else
        {
            Console.WriteLine("DatabaseSeeder: Users already exist, skipping user seeding.");
        }
    }
}
