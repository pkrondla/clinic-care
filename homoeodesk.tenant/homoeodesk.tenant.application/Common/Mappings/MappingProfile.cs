using AutoMapper;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.Entities;
using HomoeoDesk.Tenant.Domain.Modules.Appointments.ValueObjects;
using HomoeoDesk.Tenant.Domain.Enums;
using HomoeoDesk.Tenant.Domain.Entities;
using HomoeoDesk.Tenant.Application.Features.Appointments.Commands.CreateAppointment;
using HomoeoDesk.Tenant.Application.Features.Appointments.Commands.UpdateAppointment;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointment;
using HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointmentStats;
using HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch;
using HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation;
using HomoeoDesk.Tenant.Application.Features.Prescriptions.Commands.CreatePrescription;

namespace HomoeoDesk.Tenant.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Appointment mappings - using GetAppointments.AppointmentDto as the main DTO
        CreateMap<Domain.Modules.Appointments.Entities.Appointment, HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments.AppointmentDto>()
            .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.Value))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src => src.Doctor))
            .ForMember(dest => dest.Patient, opt => opt.MapFrom(src => src.Patient))
            .ForMember(dest => dest.Branch, opt => opt.MapFrom(src => src.Branch));

        // CreateAppointmentCommand to Appointment
        CreateMap<CreateAppointmentCommand, Domain.Modules.Appointments.Entities.Appointment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => AppointmentDate.Create(src.AppointmentDate)))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (AppointmentType)src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AppointmentStatus.Scheduled))
            .ForMember(dest => dest.TokenNumber, opt => opt.MapFrom(src => src.TokenNumber ?? 0)) // Will be set by handler
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Doctor, opt => opt.Ignore())
            .ForMember(dest => dest.Patient, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.Consultation, opt => opt.Ignore());

        // UpdateAppointmentCommand to Appointment
        CreateMap<UpdateAppointmentCommand, Domain.Modules.Appointments.Entities.Appointment>()
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
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
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.Consultation, opt => opt.Ignore());

        // Doctor mappings
        CreateMap<DoctorProfile, HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments.DoctorDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => src.Qualification));

        // Patient mappings
        CreateMap<Patient, HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments.PatientDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.PatientCode, opt => opt.MapFrom(src => src.PatientCode));

        // Branch mappings
        CreateMap<Branch, HomoeoDesk.Tenant.Application.Features.Appointments.Queries.GetAppointments.BranchDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));

        CreateMap<Branch, HomoeoDesk.Tenant.Application.Features.Branches.Commands.CreateBranch.BranchDto>()
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.ContactPhone))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactEmail));

        // Consultation mappings - use fully qualified name to avoid ambiguity
        CreateMap<Consultation, HomoeoDesk.Tenant.Application.Features.Consultations.Commands.CreateConsultation.ConsultationDto>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null && src.Patient.User != null 
                ? $"{src.Patient.User.FirstName} {src.Patient.User.LastName}".Trim() 
                : string.Empty))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null && src.Doctor.User != null 
                ? $"{src.Doctor.User.FirstName} {src.Doctor.User.LastName}".Trim() 
                : string.Empty))
            .ForMember(dest => dest.Photos, opt => opt.Ignore()); // Photos are mapped manually in handlers

        // Prescription mappings
        CreateMap<Prescription, PrescriptionDto>()
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.Consultation != null ? src.Consultation.PatientId : 0))
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Consultation != null && src.Consultation.Patient != null && src.Consultation.Patient.User != null
                ? $"{src.Consultation.Patient.User.FirstName} {src.Consultation.Patient.User.LastName}".Trim()
                : "Unknown"))
            .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.Consultation != null ? src.Consultation.DoctorId : 0))
            .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Consultation != null && src.Consultation.Doctor != null && src.Consultation.Doctor.User != null
                ? $"{src.Consultation.Doctor.User.FirstName} {src.Consultation.Doctor.User.LastName}".Trim()
                : "Unknown"))
            .ForMember(dest => dest.PrescriptionDate, opt => opt.MapFrom(src => src.IssuedDate))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.PatientInstructions))
            .ForMember(dest => dest.Medicines, opt => opt.MapFrom(src => src.PrescriptionItems));

        // PrescriptionItem to PrescriptionMedicineDto mapping
        CreateMap<PrescriptionItem, PrescriptionMedicineDto>()
            .ForMember(dest => dest.DispensingForm, opt => opt.MapFrom(src => (int)src.DispensingForm));
    }
}
