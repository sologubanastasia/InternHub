namespace InternHub.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using InternHub.Infrastructure.Repositories;
using InternHub.Infrastructure.Services;
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthRepository,AuthRepository>();
        services.AddScoped<ICandidateRepository,CandidateRepository>();
        services.AddScoped<ICompanyRepository,CompanyRepository>();
        services.AddScoped<ICompanyDocumentRepository,CompanyDocumentRepository>();
        services.AddScoped<IApplicationRepository,ApplicationRepository>();
        services.AddScoped<IJobRepository,JobRepository>();
        services.AddScoped<IProjectRepository,ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<IProjectTechnologyRepository, ProjectTechnologyRepository>();
        services.AddScoped<ITechnologyRepository, TechnologyRepository>();
        services.AddScoped<ITeamRequestRepository, TeamRequestRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserContextService, UserContextService>();
        
        return services;
    }
}
