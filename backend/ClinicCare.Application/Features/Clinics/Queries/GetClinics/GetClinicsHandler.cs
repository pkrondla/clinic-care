using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Application.Features.Clinics.Queries.GetClinics;

public class GetClinicsHandler : IRequestHandler<GetClinicsQuery, Result<List<ClinicDto>>>
{
    private readonly IClinicRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetClinicsHandler> _logger;

    public GetClinicsHandler(IClinicRepository repository, IMapper mapper, ILogger<GetClinicsHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ClinicDto>>> Handle(GetClinicsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var clinics = await _repository.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<ClinicDto>>(clinics);

            return Result<List<ClinicDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            _logger.LogError(ex, "Failed to retrieve clinics: {Message}", ex.Message);
            return Result<List<ClinicDto>>.Failure(new[] { $"Failed to retrieve clinics: {ex.Message}" });
        }
    }
}

