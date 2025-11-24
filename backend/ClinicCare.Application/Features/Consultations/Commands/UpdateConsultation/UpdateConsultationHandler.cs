using AutoMapper;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;

namespace ClinicCare.Application.Features.Consultations.Commands.UpdateConsultation;

public class UpdateConsultationHandler : IRequestHandler<UpdateConsultationCommand, Result<ConsultationDto>>
{
    private readonly IConsultationRepository _repository;
    private readonly IMapper _mapper;

    public UpdateConsultationHandler(IConsultationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ConsultationDto>> Handle(UpdateConsultationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var consultation = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (consultation == null)
            {
                return Result<ConsultationDto>.Failure(new[] { "Consultation not found." });
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.ChiefComplaint))
                consultation.ChiefComplaint = request.ChiefComplaint;
            
            if (!string.IsNullOrWhiteSpace(request.Symptoms))
                consultation.Symptoms = request.Symptoms;
            
            if (!string.IsNullOrWhiteSpace(request.Examination))
                consultation.Examination = request.Examination;
            
            if (!string.IsNullOrWhiteSpace(request.Diagnosis))
                consultation.Diagnosis = request.Diagnosis;
            
            if (!string.IsNullOrWhiteSpace(request.TreatmentPlan))
                consultation.TreatmentPlan = request.TreatmentPlan;
            
            if (!string.IsNullOrWhiteSpace(request.Notes))
                consultation.Notes = request.Notes;
            
            if (request.ConsultationFee.HasValue)
                consultation.ConsultationFee = request.ConsultationFee.Value;

            await _repository.UpdateAsync(consultation, cancellationToken);
            var dto = _mapper.Map<ConsultationDto>(consultation);

            return Result<ConsultationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ConsultationDto>.Failure(new[] { $"Failed to update consultation: {ex.Message}" });
        }
    }
}

