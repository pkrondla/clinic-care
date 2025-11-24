using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Consultations.Commands.CreateConsultation;

public class CreateConsultationCommand : IRequest<Result<ConsultationDto>>
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    [Required]
    public string ChiefComplaint { get; set; } = string.Empty;

    public string? Symptoms { get; set; }
    public string? Examination { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public decimal ConsultationFee { get; set; }
}

public class ConsultationDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime ConsultationDate { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? Examination { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public decimal ConsultationFee { get; set; }
    public DateTime CreatedAt { get; set; }
}

