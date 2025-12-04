namespace InternHub.API.Controllers;

using InternHub.Application.Services.Project;
using InternHub.Application.DTO.Project;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net; // Додано для повноти, але не використовується

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

    // Змінено на метод, щоб уникнути потенційних проблем, хоча властивість теж працює
    private Guid GetUserId() 
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? User.FindFirst("UserId");

        if (Guid.TryParse(userIdClaim?.Value, out var userId))
        {
            return userId;
        }
        
        // Кидаємо виняток, який буде перехоплено, якщо Claim відсутній.
        // Це має бути винятковою ситуацією, оскільки [Authorize] вже перевірив автентифікацію.
        throw new InvalidOperationException("User ID claim not found or is invalid after authentication.");
    }

    // --- GET /api/projects/my ---
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ProjectListItemDto>), 200)]
    public async Task<IActionResult> GetProjects()
    {
        var ownerId = GetUserId();
        var projects = await _projectService.GetProjectsByOwnerAsync(ownerId);
        return Ok(projects);
    }
    
    // --- POST /api/projects ---
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)] // InvalidOperationException
    [ProducesResponseType(404)] // KeyNotFoundException
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        var ownerId = GetUserId();
        var projectId = await _projectService.CreateProjectAsync(ownerId, dto);
        // Використання CreatedAtAction для повернення 201 Created
        return CreatedAtAction(nameof(GetProjectId), new { projectId = projectId}, projectId); 
    }

    // --- GET /api/projects/{projectId} ---
    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectDetailsDto), 200)] 
    [ProducesResponseType(403)] // UnauthorizedAccessException
    [ProducesResponseType(404)] // KeyNotFoundException
    public async Task<IActionResult> GetProjectId(Guid projectId)
    {
        var requesterId = GetUserId();
        var project = await _projectService.GetProjetDetailsAsync(projectId, requesterId); 
        return Ok(project);
    }
    
    // --- POST /api/projects/{projectId}/members ---
    [HttpPost("{projectId}/members")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)] // InvalidOperationException (наприклад, користувач вже член)
    [ProducesResponseType(403)] // UnauthorizedAccessException (не власник)
    [ProducesResponseType(404)] // KeyNotFoundException
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid projectId, 
        [FromBody] AddMemberDto dto 
    )
    {
        var ownerId = GetUserId();
        await _projectService.AddMemberAsync(projectId, ownerId, dto);
        return NoContent(); // 204
    }
    
    // --- DELETE /api/projects/{projectId}/members/{memberId} ---
    [HttpDelete("{projectId}/members/{memberId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)] // InvalidOperationException (власник видаляє себе)
    [ProducesResponseType(404)] // KeyNotFoundException
    [ProducesResponseType(403)] // UnauthorizedAccessException (не власник)
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid projectId,
        [FromRoute] Guid memberId
    )
    {
        var requesterId = GetUserId();
        await _projectService.RemoveMemberAsync(projectId, requesterId, memberId);
        return NoContent(); // 204
    } 

    // --- PUT /api/projects/{projectId} ---
    [HttpPut("{projectId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)] // UnauthorizedAccessException (не власник)
    public async Task<IActionResult> UpdateProject(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectDto updateDto
    )
    {
        var ownerId = GetUserId();
        await _projectService.UpdateProjectAsync(projectId, ownerId, updateDto);
        return NoContent(); // 204
    }
}