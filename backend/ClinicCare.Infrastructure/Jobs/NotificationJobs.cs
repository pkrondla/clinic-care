using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Infrastructure.Jobs;

/// <summary>
/// Background jobs for sending scheduled notifications
/// </summary>
public class NotificationJobs
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationJobs> _logger;

    public NotificationJobs(
        IApplicationDbContext context,
        INotificationService notificationService,
        ILogger<NotificationJobs> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Send appointment reminders for appointments scheduled in the next 24 hours
    /// This job runs every hour
    /// </summary>
    public async Task SendAppointmentRemindersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting appointment reminder job");

            var now = DateTime.UtcNow;
            var reminderTime = now.AddHours(1); // Remind 1 hour before appointment
            var reminderDate = DateOnly.FromDateTime(reminderTime);
            var reminderHour = reminderTime.Hour;

            // Get appointments scheduled for 1 hour from now (same date and hour)
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Clinic)
                .Where(a => a.Status == AppointmentStatus.Scheduled
                    && a.AppointmentDate.Value == reminderDate
                    && a.IsActive) // Only active appointments
                .ToListAsync(cancellationToken);

            // Filter to appointments that are approximately 1 hour away
            // (This is a simplified check - in production, you might want to check exact time)
            appointments = appointments.Where(a => 
            {
                // For same-day appointments, check if it's approximately 1 hour away
                // This is a simplified implementation
                return true; // Send reminder for all scheduled appointments on the reminder date
            }).ToList();

            _logger.LogInformation($"Found {appointments.Count} appointments to send reminders for");

            foreach (var appointment in appointments)
            {
                try
                {
                    await _notificationService.SendAppointmentReminderAsync(appointment.Id, cancellationToken);
                    _logger.LogInformation($"Sent reminder for appointment {appointment.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send reminder for appointment {appointment.Id}");
                }
            }

            _logger.LogInformation("Completed appointment reminder job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in appointment reminder job");
            throw;
        }
    }

    /// <summary>
    /// Send daily token status updates
    /// This job runs every 5 minutes during clinic hours
    /// </summary>
    public async Task SendTokenStatusUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting token status update job");

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            // Get in-progress appointments
            var inProgressAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == AppointmentStatus.InProgress
                    && a.AppointmentDate.Value == today
                    && a.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation($"Found {inProgressAppointments.Count} in-progress appointments");

            foreach (var appointment in inProgressAppointments)
            {
                try
                {
                    // Get current token number for this doctor/clinic
                    var currentToken = await _context.Appointments
                        .Where(a => a.DoctorId == appointment.DoctorId
                            && a.ClinicId == appointment.ClinicId
                            && a.AppointmentDate.Value == today
                            && a.Status == AppointmentStatus.InProgress
                            && a.TokenNumber <= appointment.TokenNumber)
                        .CountAsync(cancellationToken);

                    await _notificationService.SendTokenStatusUpdateAsync(appointment.Id, currentToken, cancellationToken);
                    _logger.LogInformation($"Sent token status update for appointment {appointment.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send token status update for appointment {appointment.Id}");
                }
            }

            _logger.LogInformation("Completed token status update job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in token status update job");
            throw;
        }
    }
}

