using InternHub.Application.DTO.Job;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using AutoMapper;
namespace InternHub.Application.Services.Team
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRequestRepository _teamRequestRepository;
        private readonly IProjectRepository _projectRepository; 
        private readonly IProjectMemberRepository _projectMemberRepository; 
        private readonly IMapper _mapper;
        public TeamService(ITeamRequestRepository teamRequestRepository,IProjectRepository projectRepository,
                            IProjectMemberRepository projectMemberRepository, IMapper mapper)
        {
            _teamRequestRepository = teamRequestRepository;
            _projectRepository = projectRepository; 
            _projectMemberRepository = projectMemberRepository; 
            _mapper = mapper;
        }

        public async Task<IList<TeamProjectDto>> GetTeamProjectsAsync()
        {
            
        }

        public async Task<ApplyToProjectDto> ApplyToProjectAsync(Guid projectId, Guid userId, ApplyToProjectDto dto)
        {
            var candidate = await _candidate
        }

        public async Task<IList<TeamRequestDetailsDto>> GetTeamRequestDetailsAsync(Guid ownerId, Guid projectId)
        {
            
        }

        public async Task ProcessRequestAsync(Guid projectId, Guid requestId, Guid ownerId, ProcessRequestDto dto)
        {
            
        }
    } 
}       