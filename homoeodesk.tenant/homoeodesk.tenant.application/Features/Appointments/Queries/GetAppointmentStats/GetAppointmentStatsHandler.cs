using MediatR;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointmentStats
{
    public class GetAppointmentStatsHandler : IRequestHandler<GetAppointmentStatsQuery, Result<AppointmentStatsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetAppointmentStatsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AppointmentStatsDto>> Handle(GetAppointmentStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Appointments.AsQueryable();

                // Apply filters based on user role
                query = await ApplyRoleBasedFilters(query, cancellationToken);

                // Apply optional filters
                if (request.BranchId.HasValue)
                    query = query.Where(x => x.BranchId == request.BranchId.Value);

                if (request.DoctorId.HasValue)
                    query = query.Where(x => x.DoctorId == request.DoctorId.Value);

                var today = DateOnly.FromDateTime(DateTime.Today);
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisWeekEnd = thisWeekStart.AddDays(6);
                var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
                var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

                var stats = new AppointmentStatsDto
                {
                    Total = await query.CountAsync(cancellationToken),
                    Today = await query.Where(x => x.AppointmentDate.Value == today).CountAsync(cancellationToken),
                    ThisWeek = await query.Where(x => x.AppointmentDate.Value >= thisWeekStart && x.AppointmentDate.Value <= thisWeekEnd).CountAsync(cancellationToken),
                    ThisMonth = await query.Where(x => x.AppointmentDate.Value >= thisMonthStart && x.AppointmentDate.Value <= thisMonthEnd).CountAsync(cancellationToken),
                    ByStatus = new StatusStats
                    {
                        Scheduled = await query.Where(x => x.Status == AppointmentStatus.Scheduled).CountAsync(cancellationToken),
                        InProgress = await query.Where(x => x.Status == AppointmentStatus.InProgress).CountAsync(cancellationToken),
                        Completed = await query.Where(x => x.Status == AppointmentStatus.Completed).CountAsync(cancellationToken),
                        Cancelled = await query.Where(x => x.Status == AppointmentStatus.Cancelled).CountAsync(cancellationToken)
                    },
                    ByType = new TypeStats
                    {
                        InPerson = await query.Where(x => x.Type == AppointmentType.InPerson).CountAsync(cancellationToken),
                        Teleconsultation = await query.Where(x => x.Type == AppointmentType.Teleconsultation).CountAsync(cancellationToken)
                    }
                };

                return Result<AppointmentStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                return Result<AppointmentStatsDto>.Failure(new[] { ex.Message });
            }
        }

        private async Task<IQueryable<Domain.Modules.Appointments.Entities.Appointment>> ApplyRoleBasedFilters(
            IQueryable<Domain.Modules.Appointments.Entities.Appointment> query, 
            CancellationToken cancellationToken)
        {
            if (_currentUserService.Role == UserRole.Doctor)
            {
                var doctorProfile = await _context.DoctorProfiles
                    .FirstOrDefaultAsync(x => x.UserId == _currentUserService.UserId, cancellationToken);
                
                if (doctorProfile != null)
                {
                    query = query.Where(x => x.DoctorId == doctorProfile.Id);
                }
            }
            else if (_currentUserService.Role == UserRole.Patient)
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(x => x.UserId == _currentUserService.UserId, cancellationToken);
                
                if (patient != null)
                {
                    query = query.Where(x => x.PatientId == patient.Id);
                }
            }

            return query;
        }
    }
}

