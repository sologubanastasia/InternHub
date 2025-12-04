namespace InternHub.API.Controllers;
using InternHub.Application.Services.Project;
using InternHub.Application.DTO.Project;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic; // Потрібно для KeyNotFoundException

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

    private Guid GetUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                                ?? User.FindFirst("UserId");

            if (Guid.TryParse(userIdClaim?.Value, out var userId))
            {
                return userId;
            }
            // У разі невдачі аутентифікації або відсутності клейму
            throw new UnauthorizedAccessException("User ID claim not found or is invalid.");
        }
    }

    // --- GET /api/projects/my ---
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ProjectListItemDto>), 200)]
    public async Task<IActionResult> GetProjects()
    {
        try
        {
            var ownerId = GetUserId;
            var projects = await _projectService.GetProjectsByOwnerAsync(ownerId);
            return Ok(projects);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        // Інші потенційні помилки, як 500, будуть оброблені глобальним Error Handler, якщо він налаштований.
    }
    
    // --- POST /api/projects ---
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)] // Додано для KeyNotFoundException
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        try
        {
            var ownerId = GetUserId;
            var projectId = await _projectService.CreateProjectAsync(ownerId, dto);
            // Використання CreatedAtAction для повернення 201 Created
            return CreatedAtAction(nameof(GetProjectId), new { projectId = projectId}, projectId); 
        }
        catch (KeyNotFoundException ex)
        {
            // Якщо технології не знайдені (KeyNotFoundException з сервісу)
            return NotFound(new { error = ex.Message }); // 404
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message }); // 400
        }
    }

    // --- GET /api/projects/{projectId} ---
    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectDetailsDto), 200)] 
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)] 
    public async Task<IActionResult> GetProjectId(Guid projectId)
    {
        try
        {
            var requesterId = GetUserId;
            var project = await _projectService.GetProjetDetailsAsync(projectId, requesterId); 
            return Ok(project);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message }); // 404
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid(); // 403 (якщо доступ заборонено, хоча користувач авторизований)
        }
    }
    
    // --- POST /api/projects/{projectId}/members ---
    [HttpPost("{projectId}/members")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)] // Додано для KeyNotFoundException
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid projectId, 
        [FromBody] AddMemberDto dto 
    )
    {
        try
        {
            var ownerId = GetUserId;
            await _projectService.AddMemberAsync(projectId, ownerId, dto);
            return NoContent(); // 204
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid(); // 403 (Не власник проекту)
        }
        catch (InvalidOperationException ex)
        {
            // Кандидат вже є членом проекту
            return BadRequest(new { error = ex.Message }); // 400
        }
        catch (KeyNotFoundException ex)
        {
            // Проект не знайдено
            return NotFound(new { error = ex.Message }); // 404
        }
    }
    
    // --- DELETE /api/projects/{projectId}/members/{memberId} ---
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
        try
        {
            var ownerId = GetUserId;
            await _projectService.RemoveMemberAsync(projectId, ownerId, memberId);
            return NoContent(); // 204
        }
        catch (KeyNotFoundException ex)
        {
            // Проект або член не знайдено
            return NotFound(new { error = ex.Message }); // 404
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid(); // 403 (Не власник проекту)
        }
        catch (InvalidOperationException ex)
        {
            // Наприклад: "Project owner cannot be removed"
            return BadRequest(new { error = ex.Message }); // 400
        }
    } 

    // --- PUT /api/projects/{projectId} ---
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
        try
        {
            var ownerId = GetUserId;
            await _projectService.UpdateProjectAsync(projectId, ownerId, updateDto);
            return NoContent(); // 204
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message }); // 404
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid(); // 403 (Не власник проекту)
        }
    }
}