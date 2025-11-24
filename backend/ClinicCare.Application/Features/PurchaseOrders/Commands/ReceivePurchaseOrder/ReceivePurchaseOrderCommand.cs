using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommand : IRequest<Result<PurchaseOrderDto>>
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item must be received")]
    public List<ReceivedItemCommand> ReceivedItems { get; set; } = new();
}

public class ReceivedItemCommand
{
    [Required]
    public int PurchaseOrderItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Received quantity must be greater than 0")]
    public int ReceivedQuantity { get; set; }

    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

