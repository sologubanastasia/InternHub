namespace InternHub.API.Controllers;

using InternHub.Application.Services.Project;
using InternHub.Application.DTO.Project;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

[Authorize]
[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("UserId");

        if (Guid.TryParse(userIdClaim?.Value, out var userId) && userId != Guid.Empty)
        {
            return userId;
        }

        // Кидаємо виняток, оскільки користувач має бути автентифікований
        // завдяки [Authorize], але Claim відсутній або недійсний.
        // Це змусить нас шукати проблему в тестовому конфігураторі.
        throw new InvalidOperationException("User ID claim not found or is invalid for an authenticated user.");
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ProjectListItemDto>), 200)]
    public async Task<IActionResult> GetProjects()
    {
        var ownerId = GetUserId();
        var projects = await _projectService.GetProjectsByOwnerAsync(ownerId);
        return Ok(projects);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)] 
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        var ownerId = GetUserId();
        var projectId = await _projectService.CreateProjectAsync(ownerId, dto);
        return CreatedAtAction(nameof(GetProjectId), new { projectId = projectId}, projectId); 
    }

    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectDetailsDto), 200)] 
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProjectId(Guid projectId)
    {
        var requesterId = GetUserId();
        var project = await _projectService.GetProjetDetailsAsync(projectId, requesterId); 
        return Ok(project);
    }
    
    [HttpPost("{projectId}/members")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(403)] 
    [ProducesResponseType(404)] 
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid projectId, 
        [FromBody] AddMemberDto dto 
    )
    {
        var ownerId = GetUserId();
        await _projectService.AddMemberAsync(projectId, ownerId, dto);
        return NoContent(); 
    }
    
    [HttpDelete("{projectId}/members/{memberId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(404)]
    [ProducesResponseType(403)] 
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid projectId,
        [FromRoute] Guid memberId
    )
    {
        var requesterId = GetUserId();
        await _projectService.RemoveMemberAsync(projectId, requesterId, memberId);
        return NoContent(); 
    } 

    [HttpPut("{projectId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)] 
    public async Task<IActionResult> UpdateProject(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectDto updateDto
    )
    {
        var ownerId = GetUserId();
        await _projectService.UpdateProjectAsync(projectId, ownerId, updateDto);
        return NoContent(); 
    }
}