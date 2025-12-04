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
        private readonly ICandidateRepository _candidateRepository;
        private readonly IMapper _mapper;
        public TeamService(ITeamRequestRepository teamRequestRepository,IProjectRepository projectRepository,
                            IProjectMemberRepository projectMemberRepository, ICandidateRepository candidateRepository, IMapper mapper)
        {
            _teamRequestRepository = teamRequestRepository;
            _projectRepository = projectRepository; 
            _projectMemberRepository = projectMemberRepository; 
            _candidateRepository = candidateRepository;
            _mapper = mapper;
        }

        public async Task<IList<TeamProjectDto>> GetTeamProjectsAsync()
        {
            
        }

        public async Task ApplyToProjectAsync(Guid projectId, Guid userId, ApplyToProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if(project == null)
                throw new KeyNotFoundException("Project not found");
        
            bool member = await _projectMemberRepository.IsMemberInProjectAsync(projectId, userId);

            if(member)
                throw new InvalidOperationException("Candidate is already a project member");

            var request = await _teamRequestRepository.HasCandidateApliedAsync(projectId, userId);

            if(request)
                throw new InvalidOperationException("Candidate already has a pending application for this project");
        
            var newRequest = _mapper.Map<TeamRequest>(dto);
            newRequest.ProjectId = projectId;
            newRequest.CandidateId = userId;
            newRequest.Status = ApplicationStatus.Pending;
            newRequest.RequestDate = DateTime.UtcNow;
            
            await _teamRequestRepository.AddAsync(newRequest);
            await _teamRequestRepository.SaveChangesAsync();
        }

        public async Task<IList<TeamRequestDetailsDto>> GetTeamRequestDetailsAsync(Guid ownerId, Guid projectId)
        {
            var owner = await _projectRepository.IsProject
        }

        public async Task ProcessRequestAsync(Guid projectId, Guid requestId, Guid ownerId, ProcessRequestDto dto)
        {
            
        }
    } 
}       