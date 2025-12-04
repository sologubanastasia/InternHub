using InternHub.Application.DTO.Job;
using ProjectEntities = InternHub.Domain.Entities.Project;
using ProjectMember = InternHub.Domain.Entities.ProjectMember;
using ProjectTechnology = InternHub.Domain.Entities.ProjectTechnology;
using InternHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using InternHub.Application.DTO.Project;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // ВИПРАВЛЕННЯ: Додано Include для уникнення помилок 500/N+1, якщо DTO цього вимагає
        public async Task<List<ProjectListItemDto>> GetProjectsByOwnerAsync(Guid ownerId)
        {
            var projectsQuery = _projectRepository.GetProjectsQuery(ownerId)
                .Include(p => p.ProjectTechnologies)
                    .ThenInclude(pt => pt.Technology); 

            var projects = await projectsQuery.ToListAsync();
            return _mapper.Map<List<ProjectListItemDto>>(projects);
        }

        public async Task<Guid> CreateProjectAsync(Guid ownerId, CreateProjectDto dto)
        {
            if (dto.ProjectTechnologies != null && dto.ProjectTechnologies.Any())
            {
                if (!await _technologyRepository.AllExistAsync(dto.ProjectTechnologies))
                {
                    throw new KeyNotFoundException("One or more specified technologies do not exist");
                }
            }

            var project =_mapper.Map<ProjectEntities>(dto);
            project.CandidateId = ownerId;
            project.StartDate = DateTime.UtcNow;

            await _projectRepository.AddAsync(project);
            
            if (dto.ProjectTechnologies != null && dto.ProjectTechnologies.Any())
            {
                var projectTechnologies = dto.ProjectTechnologies
                    .Select(techId => new ProjectTechnology
                    {
                        ProjectId = project.Id,
                        TechnologyId = techId
                    }).ToList();

                await _projectTechnologyRepository.AddRangeAsync(projectTechnologies);
            }    
            
            var owner = new ProjectMember
            {
                ProjectId = project.Id,
                CandidateId = ownerId,
                Role = "Owner"
            };    
            await _projectMemberRepository.AddAsync(owner);
            await _projectRepository.SaveChangesAsync();
            
            return project.Id;
        }

        public async Task<ProjectDetailsDto> GetProjetDetailsAsync(Guid projectId, Guid requesterId)
        {
            var project = await _projectRepository.GetProjectDetailsByIdAsync(projectId);
            if(project == null) 
               throw new KeyNotFoundException($"Project with ID {projectId} not found");

            bool owner = project.CandidateId == requesterId;
            bool member = await _projectMemberRepository.IsMemberInProjectAsync(projectId, requesterId);

            if (!owner && !member)
                 throw new UnauthorizedAccessException("Access denied");

            return _mapper.Map<ProjectDetailsDto>(project);
        }
        
        public async Task AddMemberAsync(Guid projectId, Guid ownerId, AddMemberDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if(project == null || project.CandidateId != ownerId) 
               throw new UnauthorizedAccessException("Only project owner can add members");
            
            if(await _projectMemberRepository.IsMemberInProjectAsync(projectId, dto.CandidateId))
               throw new InvalidOperationException("Candidate is already a project member");

            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                CandidateId = dto.CandidateId,
                Role = dto.Role
            };
            await _projectMemberRepository.AddAsync(newMember);
            await _projectRepository.SaveChangesAsync();   
        }

        // ВИПРАВЛЕНО: Логіка пошуку члена проекту
        public async Task RemoveMemberAsync(Guid projectId, Guid ownerId, Guid memberId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if (project == null) 
                 throw new KeyNotFoundException($"Project with ID {projectId} not found.");
                
            if (project.CandidateId != ownerId) 
                 throw new UnauthorizedAccessException("Only project owner can remove members.");

            // Використовуємо новий метод репозиторію для пошуку члена за CandidateId та ProjectId (memberId тут - це CandidateId)
            var memberToRemove = await _projectMemberRepository.GetMemberByCandidateAndProjectAsync(projectId, memberId); 

            if (memberToRemove == null)
                 throw new KeyNotFoundException("Project member not found or does not belong to this project.");

            if (memberToRemove.CandidateId == ownerId)
                 throw new InvalidOperationException("Project owner cannot be removed from the project.");

            // Викликаємо Remove, використовуючи CandidateId
            await _projectMemberRepository.Remove(projectId, memberToRemove.CandidateId);

            await _projectRepository.SaveChangesAsync(); 
        }
        
        public async Task UpdateProjectAsync(Guid projectId, Guid ownerId, UpdateProjectDto updateDto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);

            if(project == null) 
                 throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            if(project.CandidateId != ownerId) 
               throw new UnauthorizedAccessException("Only project owner can update the project");

            _mapper.Map(updateDto, project);

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();
        }
    } 
}