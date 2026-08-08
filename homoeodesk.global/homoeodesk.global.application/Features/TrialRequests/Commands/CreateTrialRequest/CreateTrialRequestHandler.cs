using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Domain.Entities;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;

namespace HomoeoDesk.Global.Application.Features.TrialRequests.Commands.CreateTrialRequest;

public class CreateTrialRequestHandler : IRequestHandler<CreateTrialRequestCommand, Result<TrialRequestDto>>
{
    private readonly IGlobalDbContext _context;

    public CreateTrialRequestHandler(IGlobalDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TrialRequestDto>> Handle(CreateTrialRequestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = new TrialRequest
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                ClinicName = request.ClinicName.Trim(),
                City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
                PatientsPerWeek = string.IsNullOrWhiteSpace(request.PatientsPerWeek) ? null : request.PatientsPerWeek.Trim(),
                Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
                Source = string.IsNullOrWhiteSpace(request.Source) ? "website" : request.Source.Trim(),
                Status = TrialRequestStatus.New,
                IsActive = true
            };

            _context.TrialRequests.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<TrialRequestDto>.Success(Map(entity));
        }
        catch (Exception ex)
        {
            return Result<TrialRequestDto>.Failure($"Failed to create trial request: {ex.Message}");
        }
    }

    internal static TrialRequestDto Map(TrialRequest entity) => new()
    {
        Id = entity.Id,
        FullName = entity.FullName,
        Email = entity.Email,
        Phone = entity.Phone,
        ClinicName = entity.ClinicName,
        City = entity.City,
        PatientsPerWeek = entity.PatientsPerWeek,
        Message = entity.Message,
        Source = entity.Source,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt
    };
}
