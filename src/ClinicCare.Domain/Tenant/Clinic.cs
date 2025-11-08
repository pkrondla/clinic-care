using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Tenant;

public class Clinic : BaseTenantEntity
{
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string Phone { get; private set; }
    public string Email { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public ICollection<Doctor> Doctors { get; private set; } = new List<Doctor>();
    public ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();

    private Clinic() { } // For EF Core

    public static Clinic Create(
        string tenantId,
        string name,
        string address,
        string phone,
        string email)
    {
        return new Clinic
        {
            TenantId = tenantId,
            Name = name,
            Address = address,
            Phone = phone,
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string address,
        string phone,
        string email)
    {
        Name = name;
        Address = address;
        Phone = phone;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}