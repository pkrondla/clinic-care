using ClinicCare.Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierCommand : IRequest<Result<bool>>
{
    [Required]
    public int Id { get; set; }
}

