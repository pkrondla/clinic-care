using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommand : IRequest<Result<PatientDto>>
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string MedicalHistory { get; set; } = string.Empty;
}

