using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Project
{
    public interface IProjectService
    {
        Task<List<ProjectListItemDto>> GetProjectsAsync();
        Task<Guid> CreateProjectAsync(Guid ownerId, CreateProjectDto createDto);
        Task<ProjectDetailsDto> GetProjetDetailsAsync(Guid projectId, Guid requesterId);
        Task AddMemberAsync(Guid projectId, Guid ownerId, AddMemberDto dto);
        Task RemoveMemberAsync(Guid projectId, Guid ownerId, Guid memberId);
        Task UpdateProjectAsync(Guid projectId, Guid ownerId, UpdateProjectDto updateDto);

    } 
}       