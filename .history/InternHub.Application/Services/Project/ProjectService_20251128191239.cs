using InternHub.Application.DTO.Job;
using InternHub.Domain.Entities;
using InternHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using InternHub.Application.DTO.Project;
using AutoMapper;
namespace InternHub.Application.Services.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectTechnologyRepository _projectTechnologyRepository;
        private readonly ITechnologyRepository _technologyRepository;
        private readonly IMapper _mapper;
        public ProjectService(IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository,
            IProjectTechnologyRepository projectTechnologyRepository, ITechnologyRepository technologyRepository,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _projectTechnologyRepository = projectTechnologyRepository;
            _technologyRepository = technologyRepository;
            _mapper = mapper;
        }

         public async Task<List<ProjectListItemDto>> GetProjectsAsync(Guid ownerId)
        {
            var projectsQuery = _projectRepository.GetProjectsQuery(ownerId);
            var projects = await projectsQuery.ToListAsync();
            return _mapper.Map<List<ProjectListItemDto>>(projects);
        }

        public async Task<Guid> CreateProjectAsync(Guid ownerId, CreateProjectDto dto)
        {
            var project =_mapper.Map<Project>(dto);
            project.CandidateId = ownerId;
            project.StartDate = DateTome.UtcNow;

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangeAsync();

            var projectTechnologies = dto.ProjectTechnologies
                .Select(techId => new ProjectTechnology
                {
                    ProjectId = project.Id,
                    TechnologyId = techId
                }).ToList();

            await _projectTechnologyRepository.AddRangeAsync(projectTechnologies);
            
            var owner = new ProjectMember
            {
                ProjectId = project.Id,
                CandidateId = ownerId,
                Role = "Owner"
            };    
            await _projectMemberRepository.AddAsync(owner);
            await _projectRepository.SaveChangeAsync();
            return project.Id;
        }

        public async Task<ProjectDetailsDto> GetProjetDetailsAsync(Guid projectId, Guid requesterId)
        {
            var project = await _projectRepository.GetProjectDetailsByIdAsync(projectId);
            if(project == null) 
               throw new KeyNotFoundException($"Project with ID {projectId} not found");

            if (project.CandidateId != requesterId &&
                !await _projectMemberRepository.IsCandidateMemberProjectAsync(projectId, requesterId))
                 throw new UnauthorizedAccessException("Project not found.");

            if (project == null) throw new Exception("Access denied, not project member");

            return _mapper.Map<ProjectDetailsDto>(project);
        }
        
        public async Task AddMemberAsync(Guid projectId, Guid ownerId, AddMemberDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if(project == null || project.CandidateId != ownerId) 
               throw new UnauthorizedAccessException("Only project owner can add members");
            
            if(await _projectMemberRepository.IsCandidateMemberProjectAsync(projectId, dto.CandidateId))
               throw new InvalidOperationException("Candidate is already a project member");

            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                CandidateId = dto.CandidateId,
                Role = dto.Role
            };
            await _projectMemberRepository.AddAsync(newMember);
            await _projectRepository.SaveChangeAsync();   
        }
        public async Task RemoveMemberAsync(Guid projectId, Guid ownerId, Guid memberId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if(project == null || project.CandidateId != ownerId) 
               throw new UnauthorizedAccessException("Only project owner can remove members");

            await _projectMemberRepository.RemoveMemberAsync(projectId, memberId);
            await _projectRepository.SaveChangeAsync();   
        }
        public async Task UpdateProjectAsync(Guid projectId, Guid ownerId, UpdateProjectDto updateDto)
        {
            var project = await _projectMemberRepository.GetByIdAsync(projectId);
            if(project == null || project.CandidateId != ownerId) 
               throw new UnauthorizedAccessException("Only project owner can update the project");
            _mapper.Map(updateDto, project);
            _projectRepository.Update(project);
            await _projectRepository.SaveChangeAsync();
        }
    } 
}       