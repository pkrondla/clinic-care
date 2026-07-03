using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;

public class GetPatientPrescriptionsHandler : IRequestHandler<GetPatientPrescriptionsQuery, Result<List<PrescriptionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPatientPrescriptionsHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<PrescriptionDto>>> Handle(GetPatientPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(pat => pat!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Appointment)
                .Include(p => p.PrescriptionItems)
                .Where(p => p.Consultation != null && p.Consultation.PatientId == request.PatientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<PrescriptionDto>>(prescriptions);

            return Result<List<PrescriptionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PrescriptionDto>>.Failure(new[] { $"Failed to retrieve patient prescriptions: {ex.Message}" });
        }
    }
}
