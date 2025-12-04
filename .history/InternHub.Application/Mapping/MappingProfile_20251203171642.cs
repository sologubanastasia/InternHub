using AutoMapper;
using InternHub.Domain.Entities;
using InternHub.Application.DTO.Candidate;
using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Project;
using InternHub.Application.DTO.Auth;
using InternHub.Application.DTO.Team;
using InternHub.Application.DTO.Admin;
using InternHub.Application.DTO.Job;
using JobApplication = InternHub.Domain.Entities.Application;
using InternHub.Application.DTO.Technology;
using System.Linq; // Додано для використання .Select()

namespace InternHub.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- AUTH MAPPINGS ---
            
            CreateMap<RegisterCandidateDto, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());

            CreateMap<RegisterCompanyDto, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore());

            CreateMap<ApplicationUser, AuthResponseDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Token, opt => opt.Ignore());    

            // --- CANDIDATE MAPPINGS ---
            CreateMap<Candidate, CandidateResponseDto>();
            CreateMap<UpdateCandidateDto, Candidate>();

            // --- COMPANY MAPPINGS ---
            CreateMap<Company, CompanyResponseDto>();
            CreateMap<UpdateCompanyDto, Company>();
            CreateMap<CompanyDocument, CompanyDocumentUploadDto>().ReverseMap();

            // --- JOB MAPPINGS ---
            CreateMap<Job, JobListDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName));

            CreateMap<Job, JobDetailDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CompanyName))
                .ForMember(dest => dest.CompanyDescription, opt => opt.MapFrom(src => src.Company.Description));

            CreateMap<CreateJobDto, Job>();

            // --- JOB APPLICATION MAPPINGS ---
            CreateMap<JobApplication, JobApplicationResponseDto>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job.Title))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Job.Company.CompanyName))
                .ForMember(dest => dest.AppliedDate, opt => opt.MapFrom(src => src.AppliedDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<JobApplicationCreateDto, JobApplication>();

            // --- PROJECT MAPPINGS ---
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsComplited, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTechnologies, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectMembers, opt => opt.Ignore())
                .ForMember(dest => dest.TeamRequests, opt => opt.Ignore());
                
            CreateMap<UpdateProjectDto, Project>(); // Додано маппінг для оновлення

            // ✅ ДОДАНО: Маппінг для отримання списку проектів
            CreateMap<Project, ProjectListDto>()
                .ForMember(dest => dest.TechnologyNames, opt => opt.MapFrom(src => 
                    src.ProjectTechnologies.Select(pt => pt.Technology.Name).ToList()))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Candidate.User.UserName));
            
            // ✅ ДОДАНО: Маппінг для отримання деталей проекту
            CreateMap<Project, ProjectDetailDto>()
                .ForMember(dest => dest.Technologies, opt => opt.MapFrom(src => src.ProjectTechnologies.Select(pt => pt.Technology)))
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.ProjectMembers))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Candidate.User.UserName));

            // ✅ ДОДАНО: Маппінг члена проекту для відображення деталей
            // Цей маппінг є критичним для ProjectDetailDto
            CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Candidate.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Candidate.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Candidate.User.Email));

            // --- TEAM REQUEST MAPPINGS ---
            CreateMap<ApplyToProjectDto, TeamRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.RequestDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<TeamRequest, TeamRequestDetailsDto>()
               .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id)) 
               .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.Candidate.User.UserName)) 
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // --- TECHNOLOGY MAPPINGS ---
            CreateMap<CreateTechnologyDto, Technology>();
            CreateMap<Technology, TechnologyDto>();
            
            // ✅ ДОДАНО: Маппінг для сутності ProjectTechnology на TechnologyDto (якщо потрібно для ProjectDetailDto)
            CreateMap<ProjectTechnology, TechnologyDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TechnologyId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Technology.Name));
        }
    }
}