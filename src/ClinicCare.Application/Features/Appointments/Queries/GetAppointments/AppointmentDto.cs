namespace ClinicCare.Application.Features.Appointments.Queries.GetAppointments
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
        public ClinicDto Clinic { get; set; } = null!;
    }

    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
    }

    public class PatientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
    }

    public class ClinicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}

