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

        if (Guid.TryParse(userIdClaim?.Value, out var userId))
        {
            return userId;
        }
        
        // Оскільки [Authorize] вже перевіряє автентифікацію, це має бути логічна помилка
        // або помилка налаштування, тому тут можна кинути виняток, який буде оброблено
        // глобальним обробником винятків, або ж використати стандартний Forbid.
        return Guid.Empty; // Це викличе помилку, якщо сервіс очікує дійсний Guid
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
    [ProducesResponseType(403)] // Помилка, що повертається сервісом
    [ProducesResponseType(404)] // Помилка, що повертається сервісом
    public async Task<IActionResult> GetProjectId(Guid projectId)
    {
        var requesterId = GetUserId();
        var project = await _projectService.GetProjetDetailsAsync(projectId, requesterId); 
        
        // Примітка: Якщо сервіс повертає null або кидає виняток, 
        // це повинно бути оброблено Global Exception Filter, щоб повернути 403/404.
        // Якщо сервіс сам повертає результат, використовуйте його.
        
        return Ok(project);
    }
    
    [HttpPost("{projectId}/members")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)] // Помилка, що повертається сервісом (наприклад, член вже є)
    [ProducesResponseType(403)] // Помилка, що повертається сервісом (не власник)
    [ProducesResponseType(404)] // Помилка, що повертається сервісом
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
    [ProducesResponseType(400)] // Спроба видалити власника
    [ProducesResponseType(404)]
    [ProducesResponseType(403)] // Запитувач не власник
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
    [ProducesResponseType(403)] // Запитувач не власник
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