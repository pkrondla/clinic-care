using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Appointments.Commands.BookAppointment;

public class BookAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    [Required]
    public int ClinicId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    public int Type { get; set; } = 1; // Default to InPerson

    public string Notes { get; set; } = string.Empty;
}

