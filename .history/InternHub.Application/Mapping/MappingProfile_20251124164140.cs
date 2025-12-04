using AutoMapper;
using InternHub.Domain.Entities;
using InternHub.Application.DTO.Candidate;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Auth;
using InternHub.Application.DTO.Project;
using InternHub.Application.DTO.Team;
using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Job;
using JobApplication = InternHub.Domain.Entities.Application;
using System.Runtime.InteropServices;

namespace InternHub.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterCandidateDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());

            CreateMap<RegisterCompanyDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            CreateMap<ApplicationUser, AuthResponseDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Token, opt => opt.Ignore());    

            CreateMap<Candidate, CandidateResponseDto>();
            CreateMap<UpdateCandidateDto, Candidate>();

            CreateMap<Company, CompanyResponseDto>();
            CreateMap<UpdateCompanyDto, Company>();

            CreateMap<CompanyDocument, CompanyDocumentUploadDto>().ReverseMap();

            CreateMap<Job, JobListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName));

            CreateMap<Job, JobDetailDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName))
                .ForMember(dest => dest.CompanyDescription, opt => opt.MapFrom(src => src.Company.Description));

            CreateMap<CreateJobDto, Job>();

            CreateMap<JobApplication, JobApplicationResponseDto>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job.Title))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Job.Company.CompanyName))
                .ForMember(dest => dest.AppliedDate, opt => opt.MapFrom(src => src.AppliedDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<JobApplicationCreateDto, JobApplication>();
        }
    }
}
