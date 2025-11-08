using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _context;
    private int? _organizationId;
    private string? _subdomain;

    public TenantService(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        Console.WriteLine($"TenantService: Constructor called with HttpContextAccessor: {httpContextAccessor != null}, DbContext: {context != null}");
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        Console.WriteLine($"TenantService: Constructor completed successfully");
    }

    public int? OrganizationId => _organizationId;
    public string? Subdomain => _subdomain;

    public async Task<int> GetOrganizationIdAsync()
    {
        Console.WriteLine($"TenantService: GetOrganizationIdAsync() called");
        
        if (_organizationId.HasValue)
        {
            Console.WriteLine($"TenantService: Returning cached organization ID: {_organizationId.Value}");
            return _organizationId.Value;
        }

        Console.WriteLine($"TenantService: Getting subdomain from request...");
        var subdomain = GetSubdomainFromRequest();
        Console.WriteLine($"TenantService: Subdomain resolved: {subdomain}");
        
        // If no subdomain is found (e.g., during Swagger requests), use default
        if (string.IsNullOrEmpty(subdomain))
        {
            Console.WriteLine($"TenantService: Subdomain is null or empty, using default subdomain for development");
            subdomain = "healthcareplus"; // Default development subdomain
        }

        try
        {
            Console.WriteLine($"TenantService: Starting database operations for subdomain: {subdomain}");
            Console.WriteLine($"TenantService: Context is null: {_context == null}");
            Console.WriteLine($"TenantService: Context type: {_context?.GetType().Name ?? "null"}");
            
            // Test database connection first
            Console.WriteLine($"TenantService: Testing database connection...");
            Console.WriteLine($"TenantService: Context type: {_context.GetType().Name}");
            
            // Cast to concrete type to access Database property
            if (_context is ApplicationDbContext dbContext)
            {
                Console.WriteLine($"TenantService: Successfully cast to ApplicationDbContext");
                Console.WriteLine($"TenantService: Database provider: {dbContext.Database.ProviderName}");
                
                try
                {
                    var canConnect = await dbContext.Database.CanConnectAsync();
                    Console.WriteLine($"TenantService: Database can connect: {canConnect}");
                    
                    if (!canConnect)
                    {
                        throw new Exception("Cannot connect to database");
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"TenantService: Database connection test failed: {dbEx.GetType().Name} - {dbEx.Message}");
                    Console.WriteLine($"TenantService: Database connection stack trace: {dbEx.StackTrace}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine($"TenantService: Failed to cast to ApplicationDbContext. Context is: {_context?.GetType().Name ?? "null"}");
                throw new Exception("Invalid DbContext type");
            }
            
            // Check if Organizations table has any data
            var totalOrganizations = await _context.Organizations.CountAsync();
            Console.WriteLine($"TenantService: Total organizations in database: {totalOrganizations}");
            
            if (totalOrganizations == 0)
            {
                Console.WriteLine($"TenantService: No organizations in database, creating default organization...");
                
                // Create default organization for development
                var defaultOrganization = new ClinicCare.Domain.Entities.Organization
                {
                    Name = "Healthcare Plus",
                    Subdomain = "healthcareplus",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Organizations.Add(defaultOrganization);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"TenantService: Created default organization with ID: {defaultOrganization.Id}");
                
                // Use the default organization if subdomain matches
                if (subdomain == "healthcareplus")
                {
                    _organizationId = defaultOrganization.Id;
                    _subdomain = subdomain;
                    return _organizationId.Value;
                }
            }
            
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(x => x.Subdomain == subdomain && x.IsActive);

            Console.WriteLine($"TenantService: Query result - Organization found: {organization != null}");
            
            if (organization == null)
            {
                Console.WriteLine($"TenantService: No organization found for subdomain: {subdomain}");
                throw new UnauthorizedAccessException($"Organization not found for subdomain: {subdomain}");
            }

            Console.WriteLine($"TenantService: Organization found - ID: {organization.Id}, Name: {organization.Name}");
            
            _organizationId = organization.Id;
            _subdomain = subdomain;

            return _organizationId.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TenantService: Exception occurred: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"TenantService: Stack trace: {ex.StackTrace}");
            throw new UnauthorizedAccessException($"Error resolving tenant: {ex.Message}");
        }
    }

    public async Task<bool> IsValidTenantAsync(string subdomain)
    {
        return await _context.Organizations
            .AnyAsync(x => x.Subdomain == subdomain && x.IsActive);
    }

    private string? GetSubdomainFromRequest()
    {
        Console.WriteLine($"TenantService: GetSubdomainFromRequest() called");
        Console.WriteLine($"TenantService: _httpContextAccessor is null: {_httpContextAccessor == null}");
        
        if (_httpContextAccessor == null)
        {
            Console.WriteLine($"TenantService: HttpContextAccessor is null, returning default subdomain");
            return "healthcareplus"; // Default development subdomain
        }
        
        Console.WriteLine($"TenantService: About to access HttpContext...");
        var httpContext = _httpContextAccessor.HttpContext;
        Console.WriteLine($"TenantService: HttpContext accessed: {httpContext != null}");
        
        if (httpContext == null)
        {
            Console.WriteLine($"TenantService: HttpContext is null, returning default subdomain");
            return "healthcareplus"; // Default development subdomain
        }
        
        Console.WriteLine($"TenantService: About to access Request...");
        var request = httpContext.Request;
        Console.WriteLine($"TenantService: Request accessed: {request != null}");
        
        if (request == null)
        {
            Console.WriteLine($"TenantService: Request is null, returning null");
            return null;
        }
        
        if (request == null) 
        {
            Console.WriteLine($"TenantService: Request is null, returning null");
            return null;
        }

        var host = request.Host.Host;
        Console.WriteLine($"TenantService: Host resolved: {host}");
        
        // Handle localhost development
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || 
            host.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase))
        {
            // For localhost, use a default subdomain for development
            var devSubdomain = "healthcareplus";
            Console.WriteLine($"TenantService: Using development subdomain: {devSubdomain}");
            return devSubdomain; // Default development subdomain
        }
        
        // Extract subdomain from host
        // Example: clinic1.yourapp.com -> clinic1
        var parts = host.Split('.');
        if (parts.Length >= 3)
        {
            return parts[0]; // First part is subdomain
        }

        // For development/testing, check for subdomain in headers or query
        if (request.Headers.ContainsKey("X-Tenant-Subdomain"))
        {
            return request.Headers["X-Tenant-Subdomain"].FirstOrDefault();
        }

        if (request.Query.ContainsKey("tenant"))
        {
            return request.Query["tenant"].FirstOrDefault();
        }

        return null;
    }
}
