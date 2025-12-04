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
using System.Threading.Tasks; 

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

        public async Task<List<ProjectListItemDto>> GetProjectsByOwnerAsync(Guid ownerId)
        {
            var projectsQuery = _projectRepository.GetProjectsQuery(ownerId);
            
            // Фільтрація за власником АБО членом, використовуючи завантажені ProjectMembers
            var projects = await projectsQuery
                .Where(p => p.CandidateId == ownerId || p.ProjectMembers.Any(pm => pm.CandidateId == ownerId))
                .ToListAsync();
                
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

            // Авторизація, використовуємо завантажені ProjectMembers
            bool owner = project.CandidateId == requesterId;
            bool member = project.ProjectMembers.Any(pm => pm.CandidateId == requesterId); 

            if (!owner && !member)
                 throw new UnauthorizedAccessException("Access denied");

            return _mapper.Map<ProjectDetailsDto>(project);
        }
        
        public async Task AddMemberAsync(Guid projectId, Guid ownerId, AddMemberDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if(project == null || project.CandidateId != ownerId) 
               throw new UnauthorizedAccessException("Only project owner can add members");
            
            // Перевірка, чи кандидат вже є власником або членом
            if(project.CandidateId == dto.CandidateId || await _projectMemberRepository.IsMemberInProjectAsync(projectId, dto.CandidateId))
               throw new InvalidOperationException("Candidate is already a project member (or is the owner).");

            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                CandidateId = dto.CandidateId,
                Role = dto.Role
            };
            await _projectMemberRepository.AddAsync(newMember);
            await _projectRepository.SaveChangesAsync();   
        }

        public async Task RemoveMemberAsync(Guid projectId, Guid ownerId, Guid memberId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if (project == null) 
                 throw new KeyNotFoundException($"Project with ID {projectId} not found.");
                
            if (project.CandidateId != ownerId) 
                 throw new UnauthorizedAccessException("Only project owner can remove members.");

            var memberToRemove = await _projectMemberRepository.GetMemberByCandidateAndProjectAsync(projectId, memberId); 

            if (memberToRemove == null)
                 throw new KeyNotFoundException("Project member not found or does not belong to this project.");

            if (memberToRemove.CandidateId == ownerId)
                 throw new InvalidOperationException("Project owner cannot be removed from the project.");

            await _projectMemberRepository.Remove(projectId, memberToRemove.CandidateId);

            await _projectRepository.SaveChangesAsync(); 
        }
        
        // 💡 Логіка оновлення проекту, включаючи обробку технологій (видалення/додавання)
        public async Task UpdateProjectAsync(Guid projectId, Guid ownerId, UpdateProjectDto updateDto)
        {
            // GetByIdAsync повинен завантажувати ProjectTechnologies (виправлено у ProjectRepository)
            var project = await _projectRepository.GetByIdAsync(projectId); 

            if(project == null) 
                 throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            if(project.CandidateId != ownerId) 
                throw new UnauthorizedAccessException("Only project owner can update the project");

            // 1. Оновлення основних властивостей
            _mapper.Map(updateDto, project);

            // 2. Оновлення технологій
            if (updateDto.ProjectTechnologies != null)
            {
                if (updateDto.ProjectTechnologies.Any() && !await _technologyRepository.AllExistAsync(updateDto.ProjectTechnologies))
                {
                    throw new KeyNotFoundException("One or more specified technologies do not exist");
                }

                // A. Видалити старі зв'язки (використовуємо метод RemoveRange)
                var existingTechnologies = project.ProjectTechnologies.ToList();
                if (existingTechnologies.Any())
                {
                    _projectTechnologyRepository.RemoveRange(existingTechnologies);
                }

                // B. Створити та додати нові зв'язки
                if (updateDto.ProjectTechnologies.Any())
                {
                    var newProjectTechnologies = updateDto.ProjectTechnologies
                        .Select(techId => new ProjectTechnology
                        {
                            ProjectId = project.Id,
                            TechnologyId = techId
                        }).ToList();

                    await _projectTechnologyRepository.AddRangeAsync(newProjectTechnologies);
                }
            }
            
            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync(); 
        }
    } 
}