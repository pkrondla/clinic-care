using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Clinics.Commands.CreateClinic;
using MediatR;

namespace ClinicCare.Application.Features.Clinics.Commands.UpdateClinic;

public class UpdateClinicHandler : IRequestHandler<UpdateClinicCommand, Result<ClinicDto>>
{
    private readonly IClinicRepository _repository;
    private readonly IMapper _mapper;

    public UpdateClinicHandler(IClinicRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(UpdateClinicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (clinic == null)
            {
                return Result<ClinicDto>.Failure(new[] { $"Clinic with ID {request.Id} not found." });
            }

            // Update properties
            clinic.Name = request.Name;
            clinic.Address = request.Address ?? string.Empty;
            clinic.ContactPhone = request.Phone ?? string.Empty;
            clinic.ContactEmail = request.Email ?? string.Empty;

            if (request.IsActive.HasValue)
            {
                clinic.IsActive = request.IsActive.Value;
            }

            await _repository.UpdateAsync(clinic, cancellationToken);
            var dto = _mapper.Map<ClinicDto>(clinic);

            return Result<ClinicDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ClinicDto>.Failure(new[] { $"Failed to update clinic: {ex.Message}" });
        }
    }
}

