using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Prescriptions.Commands.CreatePrescription;
using MediatR;

namespace ClinicCare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;

public class GetPatientPrescriptionsHandler : IRequestHandler<GetPatientPrescriptionsQuery, Result<List<PrescriptionDto>>>
{
    private readonly IPrescriptionRepository _repository;
    private readonly IMapper _mapper;

    public GetPatientPrescriptionsHandler(IPrescriptionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<PrescriptionDto>>> Handle(GetPatientPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var prescriptions = await _repository.GetByPatientIdAsync(request.PatientId, cancellationToken);
            var dtos = _mapper.Map<List<PrescriptionDto>>(prescriptions);

            return Result<List<PrescriptionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PrescriptionDto>>.Failure(new[] { $"Failed to retrieve patient prescriptions: {ex.Message}" });
        }
    }
}

