namespace HomoeoDesk.Tenant.Application.Features.Patients.Queries.GetPatient;

public class PatientDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Statistics
    public int TotalAppointments { get; set; }
    public int TotalConsultations { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime? FirstVisitDate { get; set; }
    
    // Recent appointments
    public List<RecentAppointmentDto> RecentAppointments { get; set; } = new();
    
    // Recent consultations
    public List<RecentConsultationDto> RecentConsultations { get; set; } = new();
}

public class RecentAppointmentDto
{
    public int Id { get; set; }
    public int TokenNumber { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class RecentConsultationDto
{
    public int Id { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public bool HasPrescription { get; set; }
    public List<ConsultationPhotoSummaryDto>? Photos { get; set; }
}

public class ConsultationPhotoSummaryDto
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
}
