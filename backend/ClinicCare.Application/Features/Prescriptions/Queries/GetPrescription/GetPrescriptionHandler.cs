using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPrescription;

public class GetPrescriptionHandler : IRequestHandler<GetPrescriptionQuery, Result<PrescriptionDto>>
{
    private readonly IPrescriptionRepository _repository;
    private readonly IMapper _mapper;

    public GetPrescriptionHandler(IPrescriptionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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
            return Result<PrescriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PrescriptionDto>.Failure(new[] { $"Failed to retrieve prescription: {ex.Message}" });
        }
    }
}

