using MediatR;
using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments
{
    public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, Result<List<AppointmentDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetAppointmentsHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
        {
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Result<List<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Appointments
                    .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                    .Include(x => x.Patient)
                    .ThenInclude(x => x.User)
                    .Include(x => x.Branch)
                    .AsQueryable();

                // Apply filters based on user role
                query = await ApplyRoleBasedFilters(query, cancellationToken);

                // Apply optional filters
                if (request.BranchId.HasValue)
                    query = query.Where(x => x.BranchId == request.BranchId.Value);

                if (request.DoctorId.HasValue)
                    query = query.Where(x => x.DoctorId == request.DoctorId.Value);

                if (request.Date.HasValue)
                    query = query.Where(x => x.AppointmentDate.Value == request.Date.Value);

                if (request.Status.HasValue)
                    query = query.Where(x => (int)x.Status == request.Status.Value);

                var appointments = await query
                    .OrderBy(x => x.AppointmentDate.Value)
                    .ThenBy(x => x.TokenNumber)
                    .ToListAsync(cancellationToken);

                // Map to DTOs using AutoMapper
                var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);
                return Result<List<AppointmentDto>>.Success(appointmentDtos);
            }
            catch (Exception ex)
            {
                return Result<List<AppointmentDto>>.Failure(new[] { ex.Message });
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
