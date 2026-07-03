namespace HomoeoDesk.Tenant.Domain.Modules.Appointments.ValueObjects
{
    public record AppointmentTime(TimeSpan Value)
    {
        public static AppointmentTime Create(TimeSpan time)
        {
            if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
                throw new ArgumentException("Invalid appointment time");

            return new AppointmentTime(time);
        }

        public static implicit operator TimeSpan(AppointmentTime appointmentTime) => appointmentTime.Value;
        public static implicit operator AppointmentTime(TimeSpan time) => Create(time);
    }
}

