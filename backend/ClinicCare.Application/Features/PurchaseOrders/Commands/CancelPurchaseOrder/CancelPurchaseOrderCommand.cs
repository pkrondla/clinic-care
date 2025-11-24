using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderCommand : IRequest<Result<PurchaseOrderDto>>
{
    [Required]
    public int Id { get; set; }

    public string? Reason { get; set; }
}

