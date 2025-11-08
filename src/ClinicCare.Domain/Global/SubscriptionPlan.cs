using ClinicCare.Domain.Common;

namespace ClinicCare.Domain.Global;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public decimal YearlyPrice { get; private set; }
    public int MaxClinics { get; private set; }
    public int MaxDoctorsPerClinic { get; private set; }
    public bool IsActive { get; private set; }

    private SubscriptionPlan() { } // For EF Core

    public static SubscriptionPlan Create(
        string name,
        string description,
        decimal monthlyPrice,
        decimal yearlyPrice,
        int maxClinics,
        int maxDoctorsPerClinic)
    {
        return new SubscriptionPlan
        {
            Name = name,
            Description = description,
            MonthlyPrice = monthlyPrice,
            YearlyPrice = yearlyPrice,
            MaxClinics = maxClinics,
            MaxDoctorsPerClinic = maxDoctorsPerClinic,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string description,
        decimal monthlyPrice,
        decimal yearlyPrice,
        int maxClinics,
        int maxDoctorsPerClinic)
    {
        Name = name;
        Description = description;
        MonthlyPrice = monthlyPrice;
        YearlyPrice = yearlyPrice;
        MaxClinics = maxClinics;
        MaxDoctorsPerClinic = maxDoctorsPerClinic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}