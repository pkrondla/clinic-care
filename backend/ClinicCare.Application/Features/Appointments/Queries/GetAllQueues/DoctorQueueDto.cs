namespace ClinicCare.Application.Features.Appointments.Queries.GetAllQueues;

public class DoctorQueueDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int CurrentToken { get; set; } // Currently serving token
    public int TotalTokens { get; set; } // Total tokens in queue
    public int WaitingTokens { get; set; } // Tokens waiting
    public List<QueueTokenDto> Tokens { get; set; } = new();
}

public class QueueTokenDto
{
    public int TokenNumber { get; set; }
    public int AppointmentId { get; set; }
    public int Status { get; set; } // 1=Scheduled, 2=InProgress, 3=Completed, 4=Cancelled
    public string StatusText { get; set; } = string.Empty;
    public int? PatientId { get; set; } // Only if IncludePatientDetails = true
    public string? PatientName { get; set; } // Only if IncludePatientDetails = true
    public string? PatientMobile { get; set; } // Only if IncludePatientDetails = true
    public string? PatientCode { get; set; } // Only if IncludePatientDetails = true
    public DateTime CreatedAt { get; set; }
}

