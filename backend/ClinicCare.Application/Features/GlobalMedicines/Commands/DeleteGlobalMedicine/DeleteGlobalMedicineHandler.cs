using ClinicCare.Application.Common.Interfaces.Global;
using ClinicCare.Application.Common.Models;
using MediatR;

namespace ClinicCare.Application.Features.GlobalMedicines.Commands.DeleteGlobalMedicine;

public class DeleteGlobalMedicineHandler : IRequestHandler<DeleteGlobalMedicineCommand, Result<bool>>
{
    private readonly IGlobalMedicineRepository _repository;

    public DeleteGlobalMedicineHandler(IGlobalMedicineRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteGlobalMedicineCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if medicine exists
            var medicine = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (medicine == null)
            {
                return Result<bool>.Failure(new[] { $"Global medicine with ID {request.Id} not found." });
            }

            // Soft delete
            await _repository.DeleteAsync(request.Id, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new[] { $"Failed to delete global medicine: {ex.Message}" });
        }
    }
}

