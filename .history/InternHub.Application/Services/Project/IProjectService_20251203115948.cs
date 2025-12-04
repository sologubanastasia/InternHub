using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.Project;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternHub.Application.Services.Project
{
    public interface IProjectService
    {
        Task<List<ProjectListItemDto>> GetProjectsByOwnerAsync(Guid ownerId);
        Task<Guid> CreateProjectAsync(Guid ownerId, CreateProjectDto createDto);
        Task<ProjectDetailsDto> GetProjetDetailsAsync(Guid projectId, Guid requesterId);
        Task AddMemberAsync(Guid projectId, Guid ownerId, AddMemberDto dto);
        Task RemoveMemberAsync(Guid projectId, Guid ownerId, Guid memberId);
        Task UpdateProjectAsync(Guid projectId, Guid ownerId, UpdateProjectDto updateDto);
    } 
}