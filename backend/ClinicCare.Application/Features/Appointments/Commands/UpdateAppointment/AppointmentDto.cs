namespace ClinicCare.Application.Features.Appointments.Commands.UpdateAppointment
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public int TokenNumber { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
    }
}

