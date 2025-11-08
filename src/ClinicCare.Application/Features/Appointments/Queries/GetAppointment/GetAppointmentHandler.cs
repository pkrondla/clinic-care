using MediatR;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Appointments.Queries.GetAppointment
{
    public class GetAppointmentHandler : IRequestHandler<GetAppointmentQuery, Result<AppointmentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetAppointmentHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<AppointmentDto>> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                    .Include(x => x.Patient)
                    .ThenInclude(x => x.User)
                    .Include(x => x.Clinic)
                    .Include(x => x.Consultation)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (appointment == null)
                    return Result<AppointmentDto>.Failure(new[] { "Appointment not found" });

                // Check authorization
                if (!CanAccessAppointment(appointment))
                    return Result<AppointmentDto>.Failure(new[] { "Access denied" });

                var dto = new AppointmentDto
                {
                    Id = appointment.Id,
                    TokenNumber = appointment.TokenNumber,
                    AppointmentDate = appointment.AppointmentDate.Value,
                    Type = (int)appointment.Type,
                    Status = (int)appointment.Status,
                    Notes = appointment.Notes,
                    Doctor = new DoctorDto
                    {
                        Id = appointment.Doctor.Id,
                        Name = appointment.Doctor.User.FullName,
                        Qualification = appointment.Doctor.Qualification,
                        Specialization = appointment.Doctor.Specialization
                    },
                    Patient = new PatientDto
                    {
                        Id = appointment.Patient.Id,
                        Name = appointment.Patient.User.FullName,
                        PatientCode = appointment.Patient.PatientCode,
                        Age = appointment.Patient.Age,
                        Gender = appointment.Patient.Gender
                    },
                    Clinic = new ClinicDto
                    {
                        Id = appointment.Clinic.Id,
                        Name = appointment.Clinic.Name,
                        Code = appointment.Clinic.Code
                    },
                    Consultation = appointment.Consultation != null ? new ConsultationDto
                    {
                        Id = appointment.Consultation.Id,
                        ChiefComplaint = appointment.Consultation.ChiefComplaint,
                        Diagnosis = appointment.Consultation.Diagnosis,
                        ConsultationDate = appointment.Consultation.ConsultationDate
                    } : null
                };

                return Result<AppointmentDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<AppointmentDto>.Failure(new[] { ex.Message });
            }
        }

        private bool CanAccessAppointment(Domain.Modules.Appointments.Entities.Appointment appointment)
        {
            return _currentUserService.Role switch
            {
                UserRole.SuperAdmin or UserRole.Admin => true,
                UserRole.Doctor => appointment.Doctor.UserId == _currentUserService.UserId,
                UserRole.Patient => appointment.Patient.UserId == _currentUserService.UserId,
                UserRole.Staff => true, // Staff can access all appointments in their clinics
                _ => false
            };
        }
    }
}

