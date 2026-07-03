using AutoMapper;
using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Models;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Domain.Modules.Prescriptions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionHandler : IRequestHandler<CreatePrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreatePrescriptionHandler> _logger;

    public CreatePrescriptionHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        INotificationService notificationService,
        ILogger<CreatePrescriptionHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<PrescriptionDto>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating prescription for ConsultationId: {ConsultationId}, PatientId: {PatientId}, DoctorId: {DoctorId}, MedicinesCount: {MedicinesCount}",
                request.ConsultationId, request.PatientId, request.DoctorId, request.Medicines?.Count ?? 0);

            // Get organization ID
            var organizationId = _currentUserService.OrganizationId;
            if (!organizationId.HasValue)
            {
                _logger.LogWarning("User not associated with any organization");
                return Result<PrescriptionDto>.Failure(new[] { "User not associated with any organization" });
            }

            // Get consultation to retrieve OrganizationId
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.Id == request.ConsultationId, cancellationToken);
            if (consultation == null)
            {
                _logger.LogWarning("Consultation not found: {ConsultationId}", request.ConsultationId);
                return Result<PrescriptionDto>.Failure(new[] { "Consultation not found" });
            }

            _logger.LogInformation("Consultation found: Id={Id}, OrganizationId={OrganizationId}", consultation.Id, consultation.OrganizationId);

            // Generate prescription number
            var prescriptionNumber = await GeneratePrescriptionNumberAsync(cancellationToken);

            // Create prescription
            var prescription = new Prescription
            {
                OrganizationId = consultation.OrganizationId, // Set OrganizationId from consultation
                PrescriptionNumber = prescriptionNumber,
                ConsultationId = request.ConsultationId,
                IssuedDate = DateTime.UtcNow,
                Status = Domain.Enums.PrescriptionStatus.Issued,
                PatientInstructions = request.Notes ?? string.Empty,
                InternalNotes = string.Empty,
                PrescriptionItems = request.Medicines.Select(m =>
                {
                    var quantity = m.Quantity ?? 1;
                    decimal dispensedQuantity = 0;
                    
                    // Calculate dispensed quantity based on dispensing form
                    // This is the quantity that will be deducted from inventory
                    switch ((DispensingForm)m.DispensingForm)
                    {
                        case DispensingForm.Globules:
                            // Globules: quantity (containers) * containerSize (drams) * 4 drops/dram = drops
                            var containerSize = m.ContainerSize ?? 1;
                            dispensedQuantity = quantity * containerSize * 4; // drops
                            break;
                        case DispensingForm.Liquid:
                            // Liquid: Dispensed Qty in ml (same as prescribed quantity)
                            dispensedQuantity = quantity;
                            break;
                        case DispensingForm.Tonic:
                            // Tonic: Dispensed Qty same as prescribed quantity
                            dispensedQuantity = quantity;
                            break;
                        case DispensingForm.Tablets:
                            // Tablets: Dispensed Qty same as prescribed quantity
                            dispensedQuantity = quantity;
                            break;
                        case DispensingForm.Packet:
                            // Packets: 1 Packet = 5 drops
                            dispensedQuantity = quantity * 5; // drops
                            break;
                    }
                    
                    return new PrescriptionItem
                    {
                        OrganizationId = consultation.OrganizationId, // Set OrganizationId from consultation
                        MedicineId = m.MedicineId > 0 ? m.MedicineId : null, // Set to null if 0 (custom medicine)
                        MedicineName = m.MedicineName,
                        DispensingForm = (DispensingForm)m.DispensingForm,
                        Dosage = m.Dosage,
                        Frequency = m.Frequency,
                        Duration = m.Duration, // Now comes as "4 weeks" format from frontend
                        Timing = m.Timing ?? string.Empty,
                        ContainerSize = m.ContainerSize,
                        Quantity = quantity, // Prescribed quantity for patient
                        DispensedQuantity = dispensedQuantity, // Internal: quantity for inventory deduction
                        UnitPrice = 0, // Will be calculated from medicine price
                        TotalPrice = 0, // Will be calculated
                        Instructions = m.Instructions ?? string.Empty
                    };
                }).ToList()
            };

            // Inventory deduction is handled by DeductInventoryOnPrescriptionCreatedHandler,
            // triggered by this event once the prescription is committed.
            prescription.AddDomainEvent(new PrescriptionCreatedEvent(prescription));

            _logger.LogInformation("Saving prescription to database...");
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Prescription saved successfully with Id: {PrescriptionId}, Number: {PrescriptionNumber}", prescription.Id, prescription.PrescriptionNumber);

            var dto = _mapper.Map<PrescriptionDto>(prescription);

            // Send prescription created notification (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendPrescriptionCreatedNotificationAsync(dto.Id, cancellationToken);
                }
                catch
                {
                    // Ignore notification errors - don't fail prescription creation
                }
            }, cancellationToken);

            return Result<PrescriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prescription: {Message}", ex.Message);
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            return Result<PrescriptionDto>.Failure(new[] { $"Failed to create prescription: {ex.Message}" });
        }
    }

    private async Task<string> GeneratePrescriptionNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"RX{today:yyyyMMdd}";

        var lastPrescription = await _context.Prescriptions
            .Where(p => p.PrescriptionNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PrescriptionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastPrescription == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = lastPrescription.PrescriptionNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
    }
}

