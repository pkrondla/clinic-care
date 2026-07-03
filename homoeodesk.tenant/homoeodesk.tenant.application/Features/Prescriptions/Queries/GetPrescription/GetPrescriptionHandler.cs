using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Queries.GetPrescription;

public class GetPrescriptionHandler : IRequestHandler<GetPrescriptionQuery, Result<PrescriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPrescriptionHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PrescriptionDto>> Handle(GetPrescriptionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(pat => pat!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Appointment)
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (prescription == null)
            {
                return Result<PrescriptionDto>.Failure(new[] { "Prescription not found." });
            }

            var dto = _mapper.Map<PrescriptionDto>(prescription);

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.PrescriptionId == prescription.Id && i.IsActive, cancellationToken);

            dto.HasInvoice = invoice != null;
            dto.InvoiceId = invoice?.Id;
            dto.InvoiceNumber = invoice?.InvoiceNumber;

            return Result<PrescriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PrescriptionDto>.Failure(new[] { $"Failed to retrieve prescription: {ex.Message}" });
        }
    }
}
