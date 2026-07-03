namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointmentStats
{
    public class AppointmentStatsDto
    {
        public int Total { get; set; }
        public int Today { get; set; }
        public int ThisWeek { get; set; }
        public int ThisMonth { get; set; }
        public StatusStats ByStatus { get; set; } = null!;
        public TypeStats ByType { get; set; } = null!;
    }

    public class StatusStats
    {
        public int Scheduled { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class TypeStats
    {
        public int InPerson { get; set; }
        public int Teleconsultation { get; set; }
    }
}

