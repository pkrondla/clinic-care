using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Application.Common.Services;
using ClinicCare.Application.Features.Invoices.Queries.GetInvoice;
using ClinicCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDFUnit = QuestPDF.Infrastructure.Unit;

namespace ClinicCare.Infrastructure.Services;

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

        // Get organization and clinic details
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == invoice.PatientId, cancellationToken);

        var organization = patient != null
            ? await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == patient.OrganizationId, cancellationToken)
            : null;

        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == invoice.ClinicId, cancellationToken);

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
                                col.Item().Text(organization?.Name ?? "Clinic Care")
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
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                            });

                            // Header
                            table.Header(header =>
                            {
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

                            // Items
                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.Description);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
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
                    .ThenInclude(a => a.Clinic)
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
        var clinic = appointment?.Clinic;
        var organization = patient?.Organization;

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
                                col.Item().Text(organization?.Name ?? "Clinic Care")
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

                            // Items
                            foreach (var item in prescription.PrescriptionItems)
                            {
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

