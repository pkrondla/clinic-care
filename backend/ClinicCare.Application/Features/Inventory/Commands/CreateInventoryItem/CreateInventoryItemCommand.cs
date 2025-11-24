using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Features.Inventory.Commands.AdjustStock;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ClinicCare.Application.Features.Inventory.Commands.CreateInventoryItem;

public class CreateInventoryItemCommand : IRequest<Result<InventoryDto>>
{
    public int ClinicId { get; set; }

    [Required]
    public int MedicineId { get; set; } // ClinicMedicineId

    [Required]
    public int InitialStock { get; set; }

    [Required]
    public int MinimumStock { get; set; }

    public int MaximumStock { get; set; }

    [Required]
    public decimal PurchasePrice { get; set; }

    [Required]
    public decimal SellingPrice { get; set; }

    public DateOnly ExpiryDate { get; set; }

    [MaxLength(50)]
    public string? BatchNumber { get; set; }
}

