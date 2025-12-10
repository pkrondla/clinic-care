using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPrescription;

public class GetPrescriptionHandler : IRequestHandler<GetPrescriptionQuery, Result<PrescriptionDto>>
{
    private readonly IPrescriptionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetPrescriptionHandler(IPrescriptionRepository repository, IMapper mapper, IApplicationDbContext context)
    {
        _repository = repository;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PrescriptionDto>> Handle(GetPrescriptionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var prescription = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            if (prescription == null)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Prescription not found." });
            }

            var dto = _mapper.Map<PrescriptionDto>(prescription);
            
            // Check if invoice exists for this prescription
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.PrescriptionId == prescription.Id && i.IsActive, cancellationToken);
            
            dto.HasInvoice = invoice != null;
            dto.InvoiceId = invoice?.Id;
            
            return Result<PrescriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PrescriptionDto>.Failure(new[] { $"Failed to retrieve prescription: {ex.Message}" });
        }
    }
}

