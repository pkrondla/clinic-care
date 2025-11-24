using AutoMapper;
using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using MediatR;

namespace ClinicCare.Application.Features.Clinics.Commands.CreateClinic;

public class CreateClinicHandler : IRequestHandler<CreateClinicCommand, Result<ClinicDto>>
{
    private readonly IClinicRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateClinicHandler(
        IClinicRepository repository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(CreateClinicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (organizationId == null)
            {
                return Result<ClinicDto>.Failure(new[] { "User is not associated with any organization." });
            }

            // Check if code already exists
            if (await _repository.CodeExistsAsync(request.Code, null, cancellationToken))
            {
                return Result<ClinicDto>.Failure(new[] { $"Clinic code '{request.Code}' already exists." });
            }

            // Create clinic
            var clinic = new Clinic
            {
                OrganizationId = organizationId.Value,
                Name = request.Name,
                Code = request.Code,
                Address = request.Address ?? string.Empty,
                ContactPhone = request.Phone ?? string.Empty,
                ContactEmail = request.Email ?? string.Empty,
                IsActive = true
            };

            var created = await _repository.AddAsync(clinic, cancellationToken);
            var dto = _mapper.Map<ClinicDto>(created);

            return Result<ClinicDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClinicDto>.Failure(new[] { $"Failed to create clinic: {ex.Message}" });
        }
    }
}

