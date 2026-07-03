using FluentValidation;

namespace HomoeoDesk.Tenant.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.ConsultationAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MedicineAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CourierCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one invoice item is required");
        RuleForEach(x => x.Items).SetValidator(new InvoiceItemCommandValidator());
    }
}

public class InvoiceItemCommandValidator : AbstractValidator<InvoiceItemCommand>
{
    public InvoiceItemCommandValidator()
    {
        RuleFor(x => x.ItemType).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
