using HomoeoDesk.Tenant.Application.Common.Interfaces;
using HomoeoDesk.Tenant.Application.Common.Services;
using HomoeoDesk.Tenant.Application.Features.Invoices.Queries.GetInvoice;
using HomoeoDesk.Tenant.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDFUnit = QuestPDF.Infrastructure.Unit;

namespace HomoeoDesk.Tenant.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public PdfService(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        // Get invoice data
        var query = new GetInvoiceQuery(invoiceId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.Succeeded || result.Data == null)
        {
            throw new InvalidOperationException($"Invoice not found: {invoiceId}");
        }

        var invoice = result.Data;

        // Get invoice entity to get OrganizationId
        var invoiceEntity = await _context.Invoices
            .Include(i => i.Prescription)
                .ThenInclude(p => p.PrescriptionItems.OrderBy(pi => pi.Id))
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        // Get patient and clinic details
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == invoice.PatientId, cancellationToken);

        // Get prescription items in order for serial number matching
        var prescriptionItems = invoiceEntity?.Prescription?.PrescriptionItems
            .OrderBy(pi => pi.Id)
            .ToList() ?? new List<Domain.Entities.PrescriptionItem>();

        var clinic = await _context.Branches
            .FirstOrDefaultAsync(c => c.Id == invoice.BranchId, cancellationToken);

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDFUnit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(clinic?.Name ?? "HomoeoDesk")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Medium);
                                
                                if (clinic != null)
                                {
                                    col.Item().Text(clinic.Name).FontSize(12);
                                    if (!string.IsNullOrEmpty(clinic.Address))
                                    {
                                        col.Item().Text(clinic.Address).FontSize(10);
                                    }
                                    if (!string.IsNullOrEmpty(clinic.ContactPhone))
                                    {
                                        col.Item().Text($"Phone: {clinic.ContactPhone}").FontSize(10);
                                    }
                                    if (!string.IsNullOrEmpty(clinic.ContactEmail))
                                    {
                                        col.Item().Text($"Email: {clinic.ContactEmail}").FontSize(10);
                                    }
                                }
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignRight().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                                col.Item().AlignRight().Text($"Invoice #: {invoice.InvoiceNumber}").FontSize(12).Bold();
                                col.Item().AlignRight().Text($"Date: {invoice.InvoiceDate:dd/MM/yyyy}").FontSize(10);
                                if (!string.IsNullOrEmpty(invoice.PrescriptionNumber))
                                {
                                    col.Item().AlignRight().Text($"Prescription: {invoice.PrescriptionNumber}").FontSize(10);
                                }
                            });
                        });
                    });

                page.Content()
                    .PaddingVertical(1f, QuestPDFUnit.Centimetre)
                    .Column(column =>
                    {
                        // Bill To Section
                        column.Item().PaddingBottom(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                        {
                            col.Item().Text("Bill To:").FontSize(12).Bold();
                            col.Item().Text(invoice.PatientName).FontSize(11);
                            col.Item().Text($"Patient Code: {invoice.PatientCode}").FontSize(10);
                        });

                        column.Item().PaddingVertical(0.5f, QuestPDFUnit.Centimetre);

                        // Invoice Items Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);  // Serial Number
                                columns.RelativeColumn(4);   // Description (increased)
                                columns.ConstantColumn(60);  // Qty (3 digits: 100)
                                columns.ConstantColumn(90);   // Unit Price (999.00)
                                columns.ConstantColumn(100);  // Total (9999.00)
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).AlignCenter().Text("S.No").Bold();
                                header.Cell().Element(CellStyle).Text("Description").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Unit Price").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Total").Bold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(5);
                                }
                            });

                            // Items - Combine prescription items and other invoice items
                            // Invoice items are created in order: Consultation, Medicines (in prescription order), Courier
                            int serialNumber = 0;
                            int medicineSerialNumber = 0;
                            int prescriptionItemIndex = 0;

                            // Helper function to get dispensing form name
                            string GetDispensingFormName(Domain.Enums.DispensingForm form)
                            {
                                return form switch
                                {
                                    Domain.Enums.DispensingForm.Globules => "Globules",
                                    Domain.Enums.DispensingForm.Tablets => "Tablets",
                                    Domain.Enums.DispensingForm.Packet => "Packet",
                                    Domain.Enums.DispensingForm.Liquid => "Liquid",
                                    Domain.Enums.DispensingForm.Tonic => "Tonic",
                                    _ => "Unknown"
                                };
                            }

                            // Helper function to format quantity
                            string FormatQuantity(int? quantity, Domain.Enums.DispensingForm form)
                            {
                                if (quantity == null) return "-";
                                if (form == Domain.Enums.DispensingForm.Liquid || form == Domain.Enums.DispensingForm.Tonic)
                                {
                                    return $"{quantity} ml";
                                }
                                return quantity.ToString() ?? "-";
                            }

                            foreach (var item in invoice.Items)
                            {
                                string descriptionText = "";
                                string detailsText = "";
                                
                                // For medicine items, build description with prescription details
                                if (item.ItemType == "Medicine")
                                {
                                    medicineSerialNumber++;
                                    serialNumber = medicineSerialNumber;
                                    
                                    if (prescriptionItemIndex < prescriptionItems.Count)
                                    {
                                        var prescriptionItem = prescriptionItems[prescriptionItemIndex];
                                        prescriptionItemIndex++;
                                        
                                        // Build description: Medicine #X (Form)
                                        var formName = GetDispensingFormName(prescriptionItem.DispensingForm);
                                        descriptionText = $"Medicine #{serialNumber}";
                                        if (prescriptionItem.ContainerSize.HasValue && prescriptionItem.DispensingForm == Domain.Enums.DispensingForm.Globules)
                                        {
                                            descriptionText += $" ({formName} {prescriptionItem.ContainerSize} dram)";
                                        }
                                        else
                                        {
                                            descriptionText += $" ({formName})";
                                        }
                                        
                                        // Build details: Dosage, Frequency, Timing, Duration (comma-separated)
                                        var detailsParts = new List<string>();
                                        if (!string.IsNullOrEmpty(prescriptionItem.Dosage))
                                            detailsParts.Add(prescriptionItem.Dosage);
                                        if (!string.IsNullOrEmpty(prescriptionItem.Frequency))
                                            detailsParts.Add(prescriptionItem.Frequency);
                                        if (!string.IsNullOrEmpty(prescriptionItem.Timing))
                                            detailsParts.Add(prescriptionItem.Timing);
                                        if (!string.IsNullOrEmpty(prescriptionItem.Duration))
                                            detailsParts.Add(prescriptionItem.Duration);
                                        
                                        detailsText = detailsParts.Count > 0 ? string.Join(", ", detailsParts) : "-";
                                        
                                        // Add instructions if present
                                        if (!string.IsNullOrEmpty(prescriptionItem.Instructions))
                                        {
                                            detailsText += $"\nInstructions: {prescriptionItem.Instructions}";
                                        }
                                    }
                                    else
                                    {
                                        descriptionText = $"Medicine #{serialNumber}";
                                        detailsText = "-";
                                    }
                                }
                                else
                                {
                                    // For Consultation and Courier, use normal description
                                    serialNumber++;
                                    descriptionText = item.Description;
                                    detailsText = "-";
                                }

                                table.Cell().Element(CellStyle).AlignCenter().Text(serialNumber.ToString());
                                
                                // Description cell with merged content
                                table.Cell().Element(CellStyle).Column(col =>
                                {
                                    col.Item().Text(descriptionText).FontSize(10).Bold();
                                    if (!string.IsNullOrEmpty(detailsText) && detailsText != "-")
                                    {
                                        col.Item().PaddingTop(2).Text(detailsText).FontSize(10);
                                    }
                                });
                                
                                // For medicine items, use prescription quantity; for others, use invoice item quantity
                                string quantityText = item.Quantity.ToString();
                                if (item.ItemType == "Medicine" && prescriptionItemIndex > 0 && prescriptionItemIndex <= prescriptionItems.Count)
                                {
                                    var prescriptionItem = prescriptionItems[prescriptionItemIndex - 1];
                                    quantityText = FormatQuantity(prescriptionItem.Quantity, prescriptionItem.DispensingForm);
                                }
                                
                                table.Cell().Element(CellStyle).AlignRight().Text(quantityText);
                                table.Cell().Element(CellStyle).AlignRight().Text($"₹{item.UnitPrice:F2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"₹{item.TotalPrice:F2}");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(5);
                                }
                            }
                        });

                        column.Item().PaddingTop(0.5f, QuestPDFUnit.Centimetre);

                        // Totals
                        column.Item().AlignRight().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(150).Text("Subtotal:").FontSize(11);
                                row.ConstantItem(100).AlignRight().Text($"₹{invoice.TotalAmount:F2}").FontSize(11);
                            });
                            
                            if (invoice.PaidAmount > 0)
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(150).Text("Paid Amount:").FontSize(11).FontColor(Colors.Green.Medium);
                                    row.ConstantItem(100).AlignRight().Text($"₹{invoice.PaidAmount:F2}").FontSize(11).FontColor(Colors.Green.Medium);
                                });
                            }

                            col.Item().Row(row =>
                            {
                                row.ConstantItem(150).Text("Balance:").FontSize(12).Bold();
                                row.ConstantItem(100).AlignRight().Text($"₹{invoice.BalanceAmount:F2}").FontSize(12).Bold()
                                    .FontColor(invoice.BalanceAmount > 0 ? Colors.Red.Medium : Colors.Green.Medium);
                            });
                        });

                        column.Item().PaddingTop(1f, QuestPDFUnit.Centimetre);

                        // Payment Information
                        if (invoice.PaidAmount > 0)
                        {
                            column.Item().PaddingTop(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                            {
                                col.Item().Text("Payment Information:").FontSize(12).Bold();
                                col.Item().Text($"Payment Method: {invoice.PaymentMethod}").FontSize(10);
                                if (!string.IsNullOrEmpty(invoice.PaymentReference))
                                {
                                    col.Item().Text($"Reference: {invoice.PaymentReference}").FontSize(10);
                                }
                                if (invoice.PaymentDate.HasValue)
                                {
                                    col.Item().Text($"Payment Date: {invoice.PaymentDate.Value:dd/MM/yyyy HH:mm}").FontSize(10);
                                }
                            });
                        }

                        // Status
                        column.Item().PaddingTop(0.5f, QuestPDFUnit.Centimetre).Text($"Status: {invoice.StatusText}").FontSize(10);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Thank you for your business!")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Medium);
                    });
            });
        })
        .GeneratePdf();

        return pdfBytes;
    }

    public async Task<byte[]> GeneratePrescriptionPdfAsync(int prescriptionId, bool includeMedicineNames, CancellationToken cancellationToken = default)
    {
        // Get prescription data
        var prescription = await _context.Prescriptions
            .Include(p => p.Consultation)
                .ThenInclude(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(pat => pat.User)
            .Include(p => p.Consultation)
                .ThenInclude(c => c.Doctor)
                    .ThenInclude(d => d.User)
            .Include(p => p.Consultation)
                .ThenInclude(c => c.Appointment)
                    .ThenInclude(a => a.Branch)
            .Include(p => p.PrescriptionItems)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, cancellationToken);

        if (prescription == null)
        {
            throw new InvalidOperationException($"Prescription not found: {prescriptionId}");
        }

        var consultation = prescription.Consultation;
        var appointment = consultation?.Appointment;
        var patient = appointment?.Patient;
        var doctor = consultation?.Doctor;
        var clinic = appointment?.Branch;

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDFUnit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(clinic?.Name ?? "HomoeoDesk")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Blue.Medium);
                                
                                if (clinic != null)
                                {
                                    col.Item().Text(clinic.Name).FontSize(12);
                                    if (!string.IsNullOrEmpty(clinic.Address))
                                    {
                                        col.Item().Text(clinic.Address).FontSize(10);
                                    }
                                    if (!string.IsNullOrEmpty(clinic.ContactPhone))
                                    {
                                        col.Item().Text($"Phone: {clinic.ContactPhone}").FontSize(10);
                                    }
                                }
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignRight().Text("PRESCRIPTION").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                                col.Item().AlignRight().Text($"Prescription #: {prescription.PrescriptionNumber}").FontSize(12).Bold();
                                col.Item().AlignRight().Text($"Date: {prescription.IssuedDate:dd/MM/yyyy}").FontSize(10);
                            });
                        });
                    });

                page.Content()
                    .PaddingVertical(1f, QuestPDFUnit.Centimetre)
                    .Column(column =>
                    {
                        // Patient Information
                        column.Item().PaddingBottom(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                        {
                            col.Item().Text("Patient Information:").FontSize(12).Bold();
                            col.Item().Text(patient?.User?.FullName ?? "Unknown").FontSize(11);
                            if (patient != null)
                            {
                                col.Item().Text($"Patient Code: {patient.PatientCode}").FontSize(10);
                                var age = DateTime.Today.Year - patient.DateOfBirth.Year;
                                if (patient.DateOfBirth > DateOnly.FromDateTime(DateTime.Today.AddYears(-age))) age--;
                                col.Item().Text($"Age: {age} years").FontSize(10);
                                if (!string.IsNullOrEmpty(patient.Gender))
                                {
                                    col.Item().Text($"Gender: {patient.Gender}").FontSize(10);
                                }
                            }
                        });

                        column.Item().PaddingVertical(0.5f, QuestPDFUnit.Centimetre);

                        // Doctor Information
                        column.Item().PaddingBottom(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                        {
                            col.Item().Text("Prescribed By:").FontSize(12).Bold();
                            col.Item().Text(doctor?.User?.FullName ?? "Unknown").FontSize(11);
                            if (doctor != null)
                            {
                                if (!string.IsNullOrEmpty(doctor.Qualification))
                                {
                                    col.Item().Text(doctor.Qualification).FontSize(10);
                                }
                                if (!string.IsNullOrEmpty(doctor.RegistrationNumber))
                                {
                                    col.Item().Text($"Reg. No: {doctor.RegistrationNumber}").FontSize(10);
                                }
                            }
                        });

                        column.Item().PaddingVertical(0.5f, QuestPDFUnit.Centimetre);

                        // Diagnosis
                        if (consultation != null && !string.IsNullOrEmpty(consultation.Diagnosis))
                        {
                            column.Item().PaddingBottom(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                            {
                                col.Item().Text("Diagnosis:").FontSize(12).Bold();
                                col.Item().Text(consultation.Diagnosis).FontSize(11);
                            });
                            column.Item().PaddingVertical(0.5f, QuestPDFUnit.Centimetre);
                        }

                        // Prescription Items
                        column.Item().PaddingBottom(0.3f, QuestPDFUnit.Centimetre).Text("Medicines:").FontSize(12).Bold();
                        
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);  // Serial Number
                                if (includeMedicineNames)
                                {
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(80);
                                }
                                else
                                {
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(80);
                                }
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).AlignCenter().Text("S.No").Bold();
                                if (includeMedicineNames)
                                {
                                    header.Cell().Element(CellStyle).Text("Medicine Name").Bold();
                                }
                                else
                                {
                                    header.Cell().Element(CellStyle).Text("Medicine").Bold();
                                }
                                header.Cell().Element(CellStyle).Text("Dosage").Bold();
                                header.Cell().Element(CellStyle).Text("Frequency").Bold();
                                header.Cell().Element(CellStyle).Text("Duration").Bold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(5);
                                }
                            });

                            // Items - Order by ID to maintain consistent order
                            var orderedItems = prescription.PrescriptionItems.OrderBy(pi => pi.Id).ToList();
                            int serialNumber = 0;

                            foreach (var item in orderedItems)
                            {
                                serialNumber++;
                                table.Cell().Element(CellStyle).AlignCenter().Text(serialNumber.ToString());
                                
                                if (includeMedicineNames)
                                {
                                    table.Cell().Element(CellStyle).Text(item.MedicineName);
                                }
                                else
                                {
                                    table.Cell().Element(CellStyle).Text("Medicine");
                                }
                                table.Cell().Element(CellStyle).Text(item.Dosage);
                                table.Cell().Element(CellStyle).Text(item.Frequency);
                                table.Cell().Element(CellStyle).Text(item.Duration);

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(5);
                                }
                            }
                        });

                        column.Item().PaddingTop(0.5f, QuestPDFUnit.Centimetre);

                        // Instructions
                        if (!string.IsNullOrEmpty(prescription.PatientInstructions))
                        {
                            column.Item().PaddingTop(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                            {
                                col.Item().Text("Instructions:").FontSize(12).Bold();
                                col.Item().Text(prescription.PatientInstructions).FontSize(11);
                            });
                        }

                        // Additional Instructions from Items
                        var itemsWithInstructions = prescription.PrescriptionItems
                            .Where(item => !string.IsNullOrEmpty(item.Instructions))
                            .ToList();

                        if (itemsWithInstructions.Any())
                        {
                            column.Item().PaddingTop(0.5f, QuestPDFUnit.Centimetre).Column(col =>
                            {
                                col.Item().Text("Special Instructions:").FontSize(12).Bold();
                                foreach (var item in itemsWithInstructions)
                                {
                                    if (includeMedicineNames)
                                    {
                                        col.Item().Text($"{item.MedicineName}: {item.Instructions}").FontSize(10);
                                    }
                                    else
                                    {
                                        col.Item().Text(item.Instructions).FontSize(10);
                                    }
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("This is a computer-generated prescription. Please follow the doctor's instructions carefully.")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Medium);
                    });
            });
        })
        .GeneratePdf();

        return pdfBytes;
    }
}

