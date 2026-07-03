using HomoeoDesk.Tenant.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

/// <summary>
/// Read-only access to the global medicine catalog via GlobalConnection (no cross-slice project references).
/// </summary>
public class GlobalMedicineCatalogService : IGlobalMedicineCatalogService
{
    private readonly string? _connectionString;

    public GlobalMedicineCatalogService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("GlobalConnection");
    }

    public async Task<GlobalMedicineCatalogItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("GlobalConnection is not configured for tenant catalog import.");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, GenericName, Type, Potency, Manufacturer, Price, Description
            FROM GlobalMedicines
            WHERE Id = @Id AND IsActive = 1
            """;
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new GlobalMedicineCatalogItem(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetDecimal(6),
            reader.IsDBNull(7) ? string.Empty : reader.GetString(7));
    }
}
