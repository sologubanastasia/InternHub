using InternHub.Application.DTO.Team;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace InternHub.Application.Services.Team
{
    // Вам потрібно додати інтерфейс ITeamService з усіма цими методами
    public class TeamService : ITeamService
    {
        private readonly ITeamRequestRepository _teamRequestRepository;
        private readonly IProjectRepository _projectRepository; 
        private readonly IProjectMemberRepository _projectMemberRepository; 
        private readonly ICandidateRepository _candidateRepository;
        private readonly IMapper _mapper;
        
        public TeamService(ITeamRequestRepository teamRequestRepository, IProjectRepository projectRepository,
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
           var projects = await _projectRepository.GetActiveProjectsQuery().ToListAsync();
           return _mapper.Map<List<TeamProjectDto>>(projects); 
        }

        public async Task ApplyToProjectAsync(Guid projectId, Guid userId, ApplyToProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if(project == null)
                throw new KeyNotFoundException("Project not found");
        
            // Припускаємо, що IsMemberInProjectAsync та HasCandidateApliedAsync знаходять CandidateId всередині
            bool member = await _projectMemberRepository.IsMemberInProjectAsync(projectId, userId);

            if(member)
                throw new InvalidOperationException("Candidate is already a project member");

            bool request = await _teamRequestRepository.HasCandidateApliedAsync(projectId, userId);

            if(request)
                throw new InvalidOperationException("Candidate already has a pending application for this project");
        
            var candidate = await _candidateRepository.GetByUserIdAsync(userId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found.");

            var newRequest = _mapper.Map<TeamRequest>(dto);
            newRequest.ProjectId = projectId;
            newRequest.CandidateId = candidate.Id; // Використовуємо CandidateId
            newRequest.Status = ApplicationStatus.Pending;
            newRequest.RequestDate = DateTime.UtcNow;
            
            await _teamRequestRepository.AddAsync(newRequest);
            await _teamRequestRepository.SaveChangesAsync();
        }

        public async Task<IList<TeamRequestDetailsDto>> GetTeamRequestDetailsAsync(Guid ownerId, Guid projectId)
        {
            var owner = await _projectRepository.IsProjectOwnerAsync(projectId, ownerId);
            
            if(!owner)
                throw new UnauthorizedAccessException("Only the project owner can view requests");

            var requests = await _teamRequestRepository.GetRequestsTeamIdAsync(projectId);
            return _mapper.Map<IList<TeamRequestDetailsDto>>(requests);      
        }

        public async Task ProcessRequestAsync(Guid projectId, Guid requestId, Guid ownerId, ProcessRequestDto dto)
        {
            bool owner = await _projectRepository.IsProjectOwnerAsync(projectId, ownerId);
            
            if(!owner)
                throw new UnauthorizedAccessException("Only the project owner can process requests");
            
            var request = await _teamRequestRepository.GetByIdAsync(requestId);
            
            if(request == null || request.ProjectId != projectId)
              throw new KeyNotFoundException($"Request with ID {requestId} not found for project {projectId}."); 
        
            if (request.Status != ApplicationStatus.Pending)
                throw new InvalidOperationException("This request has already been processed.");

            request.Status = dto.Action;
            
            // ПРИМІТКА: Для перевірки члена команди використовується CandidateId з TeamRequest
            var member = await _projectMemberRepository.IsMemberInProjectAsync(projectId, request.CandidateId); 

            if(dto.Action == ApplicationStatus.Accepted)
            {
                if(member)
                    throw new InvalidOperationException("Candidate is already a project member");

                var newMember = new ProjectMember
                {
                    ProjectId = projectId,
                    CandidateId = request.CandidateId,
                    Role = dto.Role ?? "Developer"
                };
                await _projectMemberRepository.AddAsync(newMember);    
            }
            
            // Оновлюємо TeamRequest
            _teamRequestRepository.Update(request); 
            
            // Зберігаємо зміни TeamRequest та ProjectMember в одній транзакції.
            // Це виправляє помилку "Expected: Accepted / Actual: Pending"
            await _teamRequestRepository.SaveChangesAsync(); 
        }

        // ✅ ВИПРАВЛЕНО: Додано виклик .Remove(member) та SaveChangesAsync()
        public async Task RemoveMemberAsync(Guid projectId, Guid ownerUserId, Guid memberUserId)
        {
            // 1. Перевірка дозволів (власник)
            bool owner = await _projectRepository.IsProjectOwnerAsync(projectId, ownerUserId);
            if (!owner)
                throw new UnauthorizedAccessException("Only the project owner can remove members.");

            // 2. Отримання CandidateId за UserId
            var candidate = await _candidateRepository.GetByUserIdAsync(memberUserId);
            if (candidate == null)
                throw new KeyNotFoundException($"Candidate with User ID {memberUserId} not found.");
            
            var candidateId = candidate.Id;

            // 3. Знаходження та видалення члена проекту
            var member = await _projectMemberRepository.GetMemberByCandidateAndProjectAsync(projectId, candidateId);

            if (member == null)
                throw new KeyNotFoundException($"Candidate is not a member of project {projectId}.");

            // --- КЛЮЧОВЕ ВИПРАВЛЕННЯ ---
            _projectMemberRepository.Remove(member);
            await _projectMemberRepository.SaveChangesAsync();
            // ---------------------------
        }
    } 
}