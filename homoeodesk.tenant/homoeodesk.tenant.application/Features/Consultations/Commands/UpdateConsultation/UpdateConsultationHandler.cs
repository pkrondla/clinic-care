using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomoeoDesk.Tenant.Application.Features.Consultations.Commands.UpdateConsultation;

public class UpdateConsultationHandler : IRequestHandler<UpdateConsultationCommand, Result<ConsultationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateConsultationHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ConsultationDto>> Handle(UpdateConsultationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (consultation == null)
            {
                return Result<ConsultationDto>.Failure(new[] { "Consultation not found." });
            }

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

            _context.Consultations.Update(consultation);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<ConsultationDto>(consultation);

            return Result<ConsultationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ConsultationDto>.Failure(new[] { $"Failed to update consultation: {ex.Message}" });
        }
    }
}
