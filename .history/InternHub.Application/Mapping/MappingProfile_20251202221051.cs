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

namespace InternHub.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- AUTH MAPPINGS ---
            
            // ВИПРАВЛЕНО: Усунуто дублювання .ForMember(dest => dest.Name)
            CreateMap<RegisterCandidateDto, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Використовуємо Email як UserName
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)) // Залишено тільки коректне зіставлення
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());

            // ВИПРАВЛЕНО: Припустив, що CompanyName йде в Name, і усунуто дублювання
            CreateMap<RegisterCompanyDto, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Використовуємо Email як UserName
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)) // Припускаємо, що Name в DTO - це назва компанії
                .ForMember(dest => dest.Surname, opt => opt.Ignore()) // Компанія не має прізвища
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
                
            // Додайте мапінг для оновлення, якщо він потрібен
            // CreateMap<UpdateProjectDto, Project>(); 

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
               // ПОТЕНЦІЙНА ПРОБЛЕМА: src.Candidate.User може бути null. 
               // Переконайтеся, що ви виконуєте Include(tr => tr.Candidate).Include(c => c.User)
               .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.Candidate.User.UserName)) 
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // --- TECHNOLOGY MAPPINGS ---
            CreateMap<CreateTechnologyDto, Technology>();
            CreateMap<Technology, TechnologyDto>();
            
            // ВИДАЛЕНО: AutoMapper автоматично мапить IList<Technology> до IList<TechnologyDto>
            // CreateMap<IList<Technology>, IList<TechnologyDto>>();
            // CreateMap<IEnumerable<Technology>, IEnumerable<TechnologyDto>>();
        }
    }
}