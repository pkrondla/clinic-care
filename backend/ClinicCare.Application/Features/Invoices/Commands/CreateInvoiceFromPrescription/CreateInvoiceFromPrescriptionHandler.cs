using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Application.Common.Services;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Application.Features.Invoices.Commands.CreateInvoiceFromPrescription;

public class CreateInvoiceFromPrescriptionHandler : IRequestHandler<CreateInvoiceFromPrescriptionCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreateInvoiceFromPrescriptionHandler> _logger;

    public CreateInvoiceFromPrescriptionHandler(
        IApplicationDbContext context,
        IPrescriptionRepository prescriptionRepository,
        ICurrentUserService currentUserService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CreateInvoiceFromPrescriptionHandler> logger)
    {
        _context = context;
        _prescriptionRepository = prescriptionRepository;
        _currentUserService = currentUserService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceFromPrescriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InvoiceDto>.Failure("User not associated with any organization");
            }

            // 1. Get prescription with all details
            var prescription = await _prescriptionRepository.GetByIdWithDetailsAsync(request.PrescriptionId, cancellationToken);
            if (prescription == null)
            {
                return Result<InvoiceDto>.Failure("Prescription not found");
            }

            // 2. Check if invoice already exists for this prescription
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.PrescriptionId == request.PrescriptionId && i.IsActive, cancellationToken);
            if (existingInvoice != null)
            {
                return Result<InvoiceDto>.Failure("Invoice already exists for this prescription");
            }

            // 3. Get consultation and appointment details
            var consultation = prescription.Consultation;
            if (consultation == null)
            {
                return Result<InvoiceDto>.Failure("Consultation not found for this prescription");
            }

            var appointment = consultation.Appointment;
            if (appointment == null)
            {
                return Result<InvoiceDto>.Failure("Appointment not found for this consultation");
            }

            // 4. Get doctor profile for consultation fee calculation
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == consultation.DoctorId && d.OrganizationId == organizationId.Value, cancellationToken);
            if (doctor == null)
            {
                return Result<InvoiceDto>.Failure("Doctor not found");
            }

            // 5. Determine if this is a new patient or follow-up
            var isNewPatient = !await _context.Consultations
                .AnyAsync(c => c.PatientId == consultation.PatientId 
                           && c.Id != consultation.Id 
                           && c.OrganizationId == organizationId.Value, cancellationToken);

            // 6. Calculate consultation fee based on appointment type and patient type
            decimal consultationFee = appointment.Type == AppointmentType.Teleconsultation
                ? (isNewPatient ? doctor.ConsultationFeeTele : doctor.FollowupFeeTele)
                : (isNewPatient ? doctor.ConsultationFeeInPerson : doctor.FollowupFeeInPerson);

            // 7. Calculate medicine charges from prescription items
            decimal medicineAmount = 0;
            var invoiceItems = new List<InvoiceItem>();

            // Add consultation item
            invoiceItems.Add(new InvoiceItem
            {
                ItemType = "Consultation",
                Description = $"Consultation Fee - {doctor.User?.FullName ?? "Doctor"}",
                Quantity = 1,
                UnitPrice = consultationFee,
                TotalPrice = consultationFee,
                OrganizationId = organizationId.Value
            });

            // Add medicine items
            foreach (var prescriptionItem in prescription.PrescriptionItems)
            {
                decimal unitPrice = 0;
                
                // Get medicine price from clinic medicine if MedicineId is provided
                if (prescriptionItem.MedicineId.HasValue && prescriptionItem.MedicineId.Value > 0)
                {
                    var clinicMedicine = await _context.ClinicMedicines
                        .FirstOrDefaultAsync(m => m.Id == prescriptionItem.MedicineId.Value 
                                                && m.ClinicId == appointment.ClinicId 
                                                && m.OrganizationId == organizationId.Value, cancellationToken);

                    if (clinicMedicine == null)
                    {
                        return Result<InvoiceDto>.Failure($"Medicine not found: {prescriptionItem.MedicineName}");
                    }

                    unitPrice = clinicMedicine.SellingPrice;
                }
                else
                {
                    // Custom medicine without ClinicMedicine record - use price from prescription item if available
                    // Otherwise, default to 0 (free medicine or price to be set manually)
                    unitPrice = prescriptionItem.UnitPrice;
                }

                // Quantity is always set: 1 for Globules (one container), calculated value for Tablets/Packets
                var quantity = prescriptionItem.Quantity ?? 1; // Default to 1 if null (shouldn't happen)
                var totalPrice = unitPrice * quantity;

                prescriptionItem.UnitPrice = unitPrice;
                prescriptionItem.TotalPrice = totalPrice;
                medicineAmount += totalPrice;

                invoiceItems.Add(new InvoiceItem
                {
                    ItemType = "Medicine",
                    Description = $"{prescriptionItem.MedicineName} - {prescriptionItem.Dosage} ({prescriptionItem.Frequency})",
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    OrganizationId = organizationId.Value
                });
            }

            // 8. Add courier charges for teleconsultation
            decimal courierCharges = 0;
            if (appointment.Type == AppointmentType.Teleconsultation && request.CourierCharges.HasValue)
            {
                courierCharges = request.CourierCharges.Value;
                invoiceItems.Add(new InvoiceItem
                {
                    ItemType = "Courier",
                    Description = "Courier Charges",
                    Quantity = 1,
                    UnitPrice = courierCharges,
                    TotalPrice = courierCharges,
                    OrganizationId = organizationId.Value
                });
            }

            // 9. Calculate total amount
            decimal totalAmount = consultationFee + medicineAmount + courierCharges;

            // 10. Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync(organizationId.Value, cancellationToken);

            // 11. Create invoice
            var invoice = new Invoice
            {
                OrganizationId = organizationId.Value,
                ClinicId = appointment.ClinicId,
                PatientId = consultation.PatientId,
                ConsultationId = consultation.Id,
                PrescriptionId = prescription.Id,
                InvoiceNumber = invoiceNumber,
                ConsultationAmount = consultationFee,
                MedicineAmount = medicineAmount,
                CourierCharges = courierCharges,
                TotalAmount = totalAmount,
                PaidAmount = 0,
                BalanceAmount = totalAmount,
                Status = InvoiceStatus.Draft,
                PaymentMethod = string.Empty,
                PaymentReference = string.Empty,
                InvoiceDate = DateTime.UtcNow,
                PaymentDate = null,
                InvoiceItems = invoiceItems
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);

            // 12. Update prescription items with prices
            // Load prescription items fresh from context to avoid tracking conflicts
            // Use prescription ID instead of Contains to avoid SQL Server OPENJSON issues
            var prescriptionItemsToUpdate = await _context.PrescriptionItems
                .Where(pi => pi.PrescriptionId == prescription.Id)
                .ToListAsync(cancellationToken);
            
            // Update only the price properties
            foreach (var prescriptionItem in prescription.PrescriptionItems)
            {
                var itemToUpdate = prescriptionItemsToUpdate.FirstOrDefault(pi => pi.Id == prescriptionItem.Id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.UnitPrice = prescriptionItem.UnitPrice;
                    itemToUpdate.TotalPrice = prescriptionItem.TotalPrice;
                }
            }
            
            await _context.SaveChangesAsync(cancellationToken);

            // 13. Load invoice with details for response
            var invoiceWithDetails = await _context.Invoices
                .Include(i => i.Clinic)
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Prescription)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

            if (invoiceWithDetails == null)
            {
                return Result<InvoiceDto>.Failure("Failed to retrieve created invoice");
            }

            // 14. Map to DTO
            var dto = new InvoiceDto
            {
                Id = invoiceWithDetails.Id,
                ClinicId = invoiceWithDetails.ClinicId,
                ClinicName = invoiceWithDetails.Clinic?.Name ?? "Unknown",
                PatientId = invoiceWithDetails.PatientId,
                PatientName = invoiceWithDetails.Patient?.User?.FullName ?? "Unknown",
                PatientCode = invoiceWithDetails.Patient?.PatientCode ?? string.Empty,
                ConsultationId = invoiceWithDetails.ConsultationId,
                PrescriptionId = invoiceWithDetails.PrescriptionId,
                PrescriptionNumber = invoiceWithDetails.Prescription?.PrescriptionNumber ?? string.Empty,
                InvoiceNumber = invoiceWithDetails.InvoiceNumber,
                ConsultationAmount = invoiceWithDetails.ConsultationAmount,
                MedicineAmount = invoiceWithDetails.MedicineAmount,
                CourierCharges = invoiceWithDetails.CourierCharges,
                TotalAmount = invoiceWithDetails.TotalAmount,
                PaidAmount = invoiceWithDetails.PaidAmount,
                BalanceAmount = invoiceWithDetails.BalanceAmount,
                Status = (int)invoiceWithDetails.Status,
                StatusText = invoiceWithDetails.Status switch
                {
                    InvoiceStatus.Draft => "Draft",
                    InvoiceStatus.Sent => "Sent",
                    InvoiceStatus.Paid => "Paid",
                    InvoiceStatus.Cancelled => "Cancelled",
                    _ => "Unknown"
                },
                PaymentMethod = invoiceWithDetails.PaymentMethod,
                PaymentReference = invoiceWithDetails.PaymentReference,
                InvoiceDate = invoiceWithDetails.InvoiceDate,
                PaymentDate = invoiceWithDetails.PaymentDate,
                CourierDocketNumber = invoiceWithDetails.CourierDocketNumber,
                CourierCompany = invoiceWithDetails.CourierCompany,
                CourierDispatchedDate = invoiceWithDetails.CourierDispatchedDate,
                CourierTrackingUrl = invoiceWithDetails.CourierTrackingUrl,
                CourierStatus = invoiceWithDetails.CourierStatus.HasValue ? (int)invoiceWithDetails.CourierStatus.Value : null,
                CourierStatusText = invoiceWithDetails.CourierStatus switch
                {
                    CourierStatus.NotDispatched => "Not Dispatched",
                    CourierStatus.Dispatched => "Dispatched",
                    CourierStatus.InTransit => "In Transit",
                    CourierStatus.OutForDelivery => "Out for Delivery",
                    CourierStatus.Delivered => "Delivered",
                    CourierStatus.Returned => "Returned",
                    _ => null
                },
                Items = invoiceWithDetails.InvoiceItems.Select(item => new InvoiceItemDto
                {
                    Id = item.Id,
                    ItemType = item.ItemType,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            // 15. Send invoice notification (fire and forget with new scope)
            var invoiceId = dto.Id; // Capture invoice ID for background task
            _ = Task.Run(async () =>
            {
                // Create a new scope for the background task to avoid disposed context issues
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                
                try
                {
                    await notificationService.SendInvoiceNotificationAsync(invoiceId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // Log but don't fail invoice creation
                    _logger.LogWarning(ex, "Failed to send invoice notification for invoice {InvoiceId}", invoiceId);
                }
            }, CancellationToken.None);

            return Result<InvoiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice from prescription {PrescriptionId}", request.PrescriptionId);
            return Result<InvoiceDto>.Failure($"Failed to create invoice: {ex.Message}");
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync(int organizationId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV{today:yyyyMMdd}";
        
        // Get the last invoice number for today
        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix) && i.OrganizationId == organizationId)
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInvoice == null)
        {
            return $"{prefix}0001";
        }

        // Extract the sequence number and increment
        var lastNumber = lastInvoice.InvoiceNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
    }
}

