using Microsoft.Extensions.DependencyInjection;
using InternHub.Application.Services.Auth;
using InternHub.Application.Services.Admin;
using InternHub.Application.Services.Candidate;
using InternHub.Application.Services.Company;
using InternHub.Application.Services.Job;
using InternHub.Application.Mapping;
using FluentValidation;
using InternHub.Application.Services.Project;
using InternHub.Domain.Entities;
using InternHub.Application.Services.Team;
using InternHub.Application.Services.Technology;
namespace InternHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<ITechnologyService, TechnologyService>();

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
