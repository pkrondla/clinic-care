namespace ClinicCare.Domain.Modules.Appointments.ValueObjects
{
    public record AppointmentDate(DateOnly Value)
    {
        public static AppointmentDate Create(DateOnly date)
        {
            if (date < DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("Appointment date cannot be in the past");

            return new AppointmentDate(date);
        }

        public static implicit operator DateOnly(AppointmentDate appointmentDate) => appointmentDate.Value;
        public static implicit operator AppointmentDate(DateOnly date) => Create(date);
    }
}

