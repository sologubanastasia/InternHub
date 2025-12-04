using InternHub.Application.DTO.Job;
using InternHub.Application.DTO.Team;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Team
{
    public interface ITeamService
    {
        public Task<IList<TeamProjectDto>> GetTeamProjectsAsync();
        public Task<ApplyToProjectDto> ApplyToProjectAsync(Guid projectId, Guid userId, ApplyToProjectDto dto);
        public Task<IList<TeamRequestDetailsDto>> GetTeamRequestDetailsAsync(Guid ownerId, Guid projectId);
        public Task ProcessRequestAsync(Guid projectId, Guid requestId, Guid ownerId, ProcessRequestDto dto);
    } 
}       