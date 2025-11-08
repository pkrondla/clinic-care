using ClinicCare.Domain.Entities;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // Check if we already have organizations
        var hasOrganizations = await context.Organizations.AnyAsync();
        Organization defaultOrganization;
        
        if (!hasOrganizations)
        {
            Console.WriteLine("DatabaseSeeder: No organizations found, creating default organization...");
            
            // Create default organization for development
            defaultOrganization = new Organization
            {
                Name = "Healthcare Plus",
                Subdomain = "healthcareplus",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(defaultOrganization);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"DatabaseSeeder: Created default organization with ID: {defaultOrganization.Id}");
        }
        else
        {
            Console.WriteLine("DatabaseSeeder: Organizations already exist, skipping seeding.");
            defaultOrganization = await context.Organizations.FirstAsync();
        }

        // Check if we have any users
        var hasUsers = await context.Users.AnyAsync();
        
        if (!hasUsers)
        {
            Console.WriteLine("DatabaseSeeder: No users found, creating default admin user...");
            
            // Create default admin user
            var adminUser = new User
            {
                Email = "admin@healthcareplus.com",
                FirstName = "Admin",
                LastName = "User",
                Phone = "+1234567890",
                Role = UserRole.Admin,
                OrganizationId = defaultOrganization.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Password: Admin@123 (hashed)
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
