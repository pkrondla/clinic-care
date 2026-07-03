using FluentValidation;
using HomoeoDesk.Tenant.Domain.Enums;

namespace HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.ConsultationId).GreaterThan(0);
        RuleFor(x => x.Medicines).NotEmpty().WithMessage("At least one medicine is required");
        RuleForEach(x => x.Medicines).SetValidator(new PrescriptionMedicineDtoValidator());
    }
}

public class PrescriptionMedicineDtoValidator : AbstractValidator<PrescriptionMedicineDto>
{
    public PrescriptionMedicineDtoValidator()
    {
        RuleFor(x => x.MedicineName).NotEmpty();
        RuleFor(x => x.Dosage).NotEmpty();
        RuleFor(x => x.Frequency).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
        RuleFor(x => x.ContainerSize)
            .GreaterThan(0)
            .When(x => (DispensingForm)x.DispensingForm == DispensingForm.Globules)
            .WithMessage("Container size is required for globules");
    }
}
