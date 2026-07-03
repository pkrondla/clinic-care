namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointment
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int TokenNumber { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DoctorDto Doctor { get; set; } = null!;
        public PatientDto Patient { get; set; } = null!;
        public BranchDto Branch { get; set; } = null!;
        public ConsultationDto? Consultation { get; set; }
    }

    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }

    public class PatientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
    }

    public class BranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ConsultationDto
    {
        public int Id { get; set; }
        public string ChiefComplaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime ConsultationDate { get; set; }
    }
}

