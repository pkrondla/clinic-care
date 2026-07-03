using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.AddConsultationPhoto;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using HomoeoDesk.Tenant.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Queries.GetConsultation;

public class GetConsultationHandler : IRequestHandler<GetConsultationQuery, Result<ConsultationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetConsultationHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<ConsultationDto>> Handle(GetConsultationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<ConsultationDto>.Failure(new[] { "User not associated with any organization" });
            }

            // Load consultation first (no includes to avoid INNER JOIN query filter issues)
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.OrganizationId == organizationId.Value && c.IsActive, cancellationToken);
            
            if (consultation == null)
            {
                return Result<ConsultationDto>.Failure(new[] { "Consultation not found." });
            }

            // Load related entities separately with explicit queries
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == consultation.PatientId, cancellationToken);

            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == consultation.DoctorId, cancellationToken);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == consultation.AppointmentId, cancellationToken);

            var prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                .Where(p => p.ConsultationId == consultation.Id)
                .ToListAsync(cancellationToken);

            var photos = await _context.ConsultationPhotos
                .Where(p => p.ConsultationId == consultation.Id && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);

            // Manually assign navigation properties
            consultation.Patient = patient;
            consultation.Doctor = doctor;
            consultation.Appointment = appointment;
            consultation.Prescriptions = prescriptions;
            // Don't assign Photos to consultation entity to avoid AutoMapper issues
            // We'll map photos manually below

            var dto = _mapper.Map<ConsultationDto>(consultation);
            
            // Set prescription information
            dto.HasPrescription = consultation.Prescriptions != null && consultation.Prescriptions.Any(p => p.IsActive);
            dto.PrescriptionId = consultation.Prescriptions != null && consultation.Prescriptions.Any(p => p.IsActive)
                ? consultation.Prescriptions.FirstOrDefault(p => p.IsActive)?.Id
                : null;
            
            // Set photos (map from the photos variable we loaded separately)
            dto.Photos = photos.Select(p => new ConsultationPhotoDto
            {
                Id = p.Id,
                ConsultationId = p.ConsultationId,
                PhotoUrl = p.PhotoUrl,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                CreatedAt = p.CreatedAt
            }).ToList();
            
            return Result<ConsultationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            Console.WriteLine($"GetConsultation Error: {ex}");
            return Result<ConsultationDto>.Failure(new[] { $"Failed to retrieve consultation: {ex.Message}" });
        }
    }
}

