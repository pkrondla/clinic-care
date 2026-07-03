using AutoMapper;
using HomoeoDesk.Global.Application.Features.GlobalMedicines.Commands.CreateGlobalMedicine;
using HomoeoDesk.Global.Application.Features.Organizations.Commands.CreateOrganization;
using HomoeoDesk.Global.Domain.Entities;
using HomoeoDesk.Global.Domain.Enums;

namespace HomoeoDesk.Global.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GlobalTenant, OrganizationDto>()
            .ForMember(dest => dest.SubscriptionStatus, opt => opt.MapFrom(src => src.SubscriptionStatus.ToString()));

        CreateMap<GlobalMedicine, GlobalMedicineDto>();

        CreateMap<CreateOrganizationCommand, GlobalTenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DatabaseName, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionStatus, opt => opt.MapFrom(src => SubscriptionStatus.Trial))
            .ForMember(dest => dest.TrialEndDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Subscriptions, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentTransactions, opt => opt.Ignore());

        CreateMap<CreateGlobalMedicineCommand, GlobalMedicine>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty));
    }
}
