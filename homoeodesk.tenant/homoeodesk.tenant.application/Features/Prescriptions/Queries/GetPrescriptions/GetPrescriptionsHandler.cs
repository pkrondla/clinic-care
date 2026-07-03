using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescriptions;

public class GetPrescriptionsHandler : IRequestHandler<GetPrescriptionsQuery, Result<List<PrescriptionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetPrescriptionsHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<List<PrescriptionDto>>> Handle(GetPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<List<PrescriptionDto>>.Failure("User not associated with any organization");
            }

            // Handle doctorId lookup if provided (could be UserId or DoctorProfile.Id)
            int? actualDoctorId = request.DoctorId;
            if (request.DoctorId.HasValue)
            {
                // Try to find doctor by DoctorProfile.Id first, then by UserId
                var doctorProfile = await _context.DoctorProfiles
                    .FirstOrDefaultAsync(d => (d.Id == request.DoctorId.Value || d.UserId == request.DoctorId.Value)
                                           && d.OrganizationId == organizationId.Value
                                           && d.IsActive, cancellationToken);
                
                if (doctorProfile != null)
                {
                    actualDoctorId = doctorProfile.Id;
                }
                else
                {
                    // If doctor not found, return empty result
                    return Result<List<PrescriptionDto>>.Success(new List<PrescriptionDto>());
                }
            }

            var query = _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Appointment)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(pat => pat!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.PrescriptionItems)
                // Don't include Medicine navigation property to avoid ClinicMedicineId shadow property issue
                .Where(p => p.OrganizationId == organizationId.Value && p.IsActive && p.Consultation != null);

            // Apply filters
            if (request.BranchId.HasValue)
            {
                query = query.Where(p => p.Consultation != null && 
                                         p.Consultation.Appointment != null && 
                                         p.Consultation.Appointment.BranchId == request.BranchId.Value);
            }

            if (actualDoctorId.HasValue)
            {
                query = query.Where(p => p.Consultation != null && p.Consultation.DoctorId == actualDoctorId.Value);
            }

            if (request.PatientId.HasValue)
            {
                query = query.Where(p => p.Consultation != null && p.Consultation.PatientId == request.PatientId.Value);
            }

            if (request.StartDate.HasValue)
            {
                var startDateTime = request.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(p => p.IssuedDate >= startDateTime);
            }

            if (request.EndDate.HasValue)
            {
                var endDateTime = request.EndDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(p => p.IssuedDate <= endDateTime);
            }

            var prescriptions = await query
                .OrderByDescending(p => p.IssuedDate)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            // Get all prescription IDs to check for invoices
            var prescriptionIds = prescriptions.Select(p => p.Id).ToList();
            
            // Load invoices - filter by non-null PrescriptionId first to avoid OPENJSON issues
            // Convert to array to avoid EF Core OPENJSON translation issues with List
            var invoices = prescriptionIds.Count > 0
                ? await _context.Invoices
                    .Where(i => i.PrescriptionId.HasValue && i.IsActive)
                    .ToListAsync(cancellationToken)
                : new List<Domain.Entities.Invoice>();
            
            // Filter in memory to avoid SQL translation issues
            var invoiceLookup = invoices
                .Where(i => i.PrescriptionId.HasValue && prescriptionIds.Contains(i.PrescriptionId.Value))
                .ToDictionary(i => i.PrescriptionId!.Value, i => new { i.Id, i.InvoiceNumber });

            var dtos = prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                PrescriptionNumber = p.PrescriptionNumber,
                ConsultationId = p.ConsultationId,
                PatientId = p.Consultation?.PatientId ?? 0,
                PatientName = p.Consultation?.Patient?.User?.FullName ?? "Unknown",
                DoctorId = p.Consultation?.DoctorId ?? 0,
                DoctorName = p.Consultation?.Doctor?.User?.FullName ?? "Unknown",
                PrescriptionDate = p.IssuedDate,
                Medicines = p.PrescriptionItems.Select(item => new PrescriptionMedicineDto
                {
                    MedicineId = item.MedicineId,
                    MedicineName = item.MedicineName,
                    DispensingForm = (int)item.DispensingForm,
                    Dosage = item.Dosage,
                    Frequency = item.Frequency,
                    Duration = item.Duration,
                    Timing = item.Timing,
                    ContainerSize = item.ContainerSize,
                    Quantity = item.Quantity,
                    Instructions = item.Instructions
                }).ToList(),
                Notes = p.PatientInstructions,
                CreatedAt = p.CreatedAt,
                HasInvoice = invoiceLookup.ContainsKey(p.Id),
                InvoiceId = invoiceLookup.ContainsKey(p.Id) ? invoiceLookup[p.Id].Id : null,
                InvoiceNumber = invoiceLookup.ContainsKey(p.Id) ? invoiceLookup[p.Id].InvoiceNumber : null
            }).ToList();

            return Result<List<PrescriptionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PrescriptionDto>>.Failure($"Failed to retrieve prescriptions: {ex.Message}");
        }
    }
}

