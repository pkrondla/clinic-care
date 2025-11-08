namespace ClinicCare.Domain.Common
{
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public Guid Id { get; } = Guid.NewGuid();
    }
}
