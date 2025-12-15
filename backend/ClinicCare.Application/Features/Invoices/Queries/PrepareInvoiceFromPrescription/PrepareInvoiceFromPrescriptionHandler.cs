using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Interfaces.Tenant;
using ClinicCare.Application.Common.Models;
using ClinicCare.Domain.Entities;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicCare.Application.Features.Invoices.Queries.PrepareInvoiceFromPrescription;

public class PrepareInvoiceFromPrescriptionHandler 
    : IRequestHandler<PrepareInvoiceFromPrescriptionQuery, Result<InvoicePreparationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PrepareInvoiceFromPrescriptionHandler> _logger;

    public PrepareInvoiceFromPrescriptionHandler(
        IApplicationDbContext context,
        IPrescriptionRepository prescriptionRepository,
        ICurrentUserService currentUserService,
        ILogger<PrepareInvoiceFromPrescriptionHandler> logger)
    {
        _context = context;
        _prescriptionRepository = prescriptionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<InvoicePreparationDto>> Handle(
        PrepareInvoiceFromPrescriptionQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                return Result<InvoicePreparationDto>.Failure("User not associated with any organization");
            }

            // 1. Get prescription with all details
            var prescription = await _prescriptionRepository.GetByIdWithDetailsAsync(
                request.PrescriptionId, cancellationToken);
            if (prescription == null)
            {
                return Result<InvoicePreparationDto>.Failure("Prescription not found");
            }

            // 2. Check if invoice already exists for this prescription
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.PrescriptionId == request.PrescriptionId && i.IsActive, cancellationToken);
            if (existingInvoice != null)
            {
                return Result<InvoicePreparationDto>.Failure("Invoice already exists for this prescription");
            }

            // 3. Get consultation and appointment details
            var consultation = prescription.Consultation;
            if (consultation == null)
            {
                return Result<InvoicePreparationDto>.Failure("Consultation not found for this prescription");
            }

            var appointment = consultation.Appointment;
            if (appointment == null)
            {
                return Result<InvoicePreparationDto>.Failure("Appointment not found for this consultation");
            }

            // 4. Get clinic
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == appointment.ClinicId && c.OrganizationId == organizationId.Value, cancellationToken);
            if (clinic == null)
            {
                return Result<InvoicePreparationDto>.Failure("Clinic not found");
            }

            // 5. Get patient
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == consultation.PatientId && p.OrganizationId == organizationId.Value, cancellationToken);
            if (patient == null || patient.User == null)
            {
                return Result<InvoicePreparationDto>.Failure("Patient not found");
            }

            // 6. Get doctor profile for consultation fee calculation
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == consultation.DoctorId && d.OrganizationId == organizationId.Value, cancellationToken);
            if (doctor == null)
            {
                return Result<InvoicePreparationDto>.Failure("Doctor not found");
            }

            // 7. Determine if this is a new patient or follow-up
            var isNewPatient = !await _context.Consultations
                .AnyAsync(c => c.PatientId == consultation.PatientId 
                           && c.Id != consultation.Id 
                           && c.OrganizationId == organizationId.Value, cancellationToken);

            // 8. Calculate consultation fee based on appointment type and patient type
            decimal consultationFee = appointment.Type == AppointmentType.Teleconsultation
                ? (isNewPatient ? doctor.ConsultationFeeTele : doctor.FollowupFeeTele)
                : (isNewPatient ? doctor.ConsultationFeeInPerson : doctor.FollowupFeeInPerson);

            // 9. Calculate medicine charges from prescription items
            decimal medicineAmount = 0;
            var invoiceItems = new List<InvoiceItemPreparationDto>();

            // Add consultation item
            invoiceItems.Add(new InvoiceItemPreparationDto
            {
                ItemType = "Consultation",
                Description = $"Consultation Fee - {doctor.User?.FullName ?? "Doctor"}",
                Quantity = 1,
                UnitPrice = consultationFee
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

                    if (clinicMedicine != null)
                    {
                        unitPrice = clinicMedicine.SellingPrice;
                    }
                    else
                    {
                        // Medicine not found in clinic - use price from prescription item if available
                        unitPrice = prescriptionItem.UnitPrice;
                    }
                }
                else
                {
                    // Custom medicine without ClinicMedicine record
                    unitPrice = prescriptionItem.UnitPrice;
                }

                var quantity = prescriptionItem.Quantity ?? 1;
                var totalPrice = unitPrice * quantity;
                medicineAmount += totalPrice;

                invoiceItems.Add(new InvoiceItemPreparationDto
                {
                    ItemType = "Medicine",
                    Description = $"{prescriptionItem.MedicineName} - {prescriptionItem.Dosage} ({prescriptionItem.Frequency})",
                    Quantity = quantity,
                    UnitPrice = unitPrice
                });
            }

            // 10. Add courier charges item for teleconsultation (default 0, user can edit)
            decimal courierCharges = 0;
            if (appointment.Type == AppointmentType.Teleconsultation)
            {
                // Add courier item with 0 amount - user can edit the price
                invoiceItems.Add(new InvoiceItemPreparationDto
                {
                    ItemType = "Courier",
                    Description = "Courier Charges",
                    Quantity = 1,
                    UnitPrice = 0 // User can edit this
                });
            }

            var dto = new InvoicePreparationDto
            {
                ClinicId = appointment.ClinicId,
                ClinicName = clinic.Name,
                PatientId = consultation.PatientId,
                PatientName = patient.User.FullName,
                PatientCode = patient.PatientCode,
                ConsultationId = consultation.Id,
                PrescriptionId = prescription.Id,
                ConsultationAmount = consultationFee,
                MedicineAmount = medicineAmount,
                CourierCharges = courierCharges,
                Items = invoiceItems
            };

            return Result<InvoicePreparationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing invoice from prescription {PrescriptionId}", request.PrescriptionId);
            return Result<InvoicePreparationDto>.Failure($"Failed to prepare invoice: {ex.Message}");
        }
    }
}

