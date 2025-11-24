using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommand : IRequest<Result<PurchaseOrderDto>>
{
    [Required]
    public int Id { get; set; }
}

