using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Organizations.Commands.CreateOrganization;
using MediatR;

namespace ClinicCare.Application.Features.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IOrganizationRepository _repository;
    private readonly IMapper _mapper;

    public UpdateOrganizationHandler(IOrganizationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizationDto>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing organization
            var organization = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (organization == null)
            {
                return Result<OrganizationDto>.Failure(new[] { $"Organization with ID {request.Id} not found." });
            }

            // Update properties
            organization.Name = request.Name;
            organization.ContactEmail = request.ContactEmail;
            organization.ContactPhone = request.ContactPhone ?? string.Empty;
            organization.Address = request.Address ?? string.Empty;

            if (request.IsActive.HasValue)
            {
                organization.IsActive = request.IsActive.Value;
            }

            // Save changes
            await _repository.UpdateAsync(organization, cancellationToken);

            // Map to DTO
            var dto = _mapper.Map<OrganizationDto>(organization);

            return Result<OrganizationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrganizationDto>.Failure(new[] { $"Failed to update organization: {ex.Message}" });
        }
    }
}

