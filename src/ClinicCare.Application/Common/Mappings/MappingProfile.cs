using AutoMapper;
using ClinicCare.Domain.Modules.Appointments.Entities;
using ClinicCare.Domain.Modules.Appointments.ValueObjects;
using ClinicCare.Domain.Enums;
using ClinicCare.Domain.Entities;
using ClinicCare.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicCare.Application.Features.Appointments.Commands.UpdateAppointment;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointments;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointment;
using ClinicCare.Application.Features.Appointments.Queries.GetAppointmentStats;

namespace ClinicCare.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Appointment mappings - using GetAppointments.AppointmentDto as the main DTO
        CreateMap<Domain.Modules.Appointments.Entities.Appointment, ClinicCare.Application.Features.Appointments.Queries.GetAppointments.AppointmentDto>()
            .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.Value))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src => src.Doctor))
            .ForMember(dest => dest.Patient, opt => opt.MapFrom(src => src.Patient))
            .ForMember(dest => dest.Clinic, opt => opt.MapFrom(src => src.Clinic));

        // CreateAppointmentCommand to Appointment
        CreateMap<CreateAppointmentCommand, Domain.Modules.Appointments.Entities.Appointment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => AppointmentDate.Create(src.AppointmentDate)))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (AppointmentType)src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AppointmentStatus.Scheduled))
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore())
            .ForMember(dest => dest.Patient, opt => opt.Ignore())
            .ForMember(dest => dest.Clinic, opt => opt.Ignore())
            .ForMember(dest => dest.Consultation, opt => opt.Ignore());

        // UpdateAppointmentCommand to Appointment
        CreateMap<UpdateAppointmentCommand, Domain.Modules.Appointments.Entities.Appointment>()
            .ForMember(dest => dest.ClinicId, opt => opt.Ignore())
            .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
            .ForMember(dest => dest.PatientId, opt => opt.Ignore())
            .ForMember(dest => dest.AppointmentDate, opt => opt.Ignore())
            .ForMember(dest => dest.TokenNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore())
            .ForMember(dest => dest.Patient, opt => opt.Ignore())
            .ForMember(dest => dest.Clinic, opt => opt.Ignore())
            .ForMember(dest => dest.Consultation, opt => opt.Ignore());

        // Doctor mappings
        CreateMap<DoctorProfile, ClinicCare.Application.Features.Appointments.Queries.GetAppointments.DoctorDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => src.Qualification));

        // Patient mappings
        CreateMap<Patient, ClinicCare.Application.Features.Appointments.Queries.GetAppointments.PatientDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.PatientCode, opt => opt.MapFrom(src => src.PatientCode));

        // Clinic mappings
        CreateMap<Clinic, ClinicCare.Application.Features.Appointments.Queries.GetAppointments.ClinicDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));
    }
}
