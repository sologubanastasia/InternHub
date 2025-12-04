namespace InternHub.API.Controllers;
using InternHub.Application.Services.Project;
using InternHub.Application.DTO.Project;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

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
            throw new InvalidOperationException("User ID claim not found or is invalid");
        }
    }

    // --- GET /api/projects/my ---
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<ProjectListItemDto>), 200)]
    public async Task<IActionResult> GetProjects()
    {
        var ownerId = GetUserId;
        var projects = await _projectService.GetProjectsByOwnerAsync(ownerId);
        return Ok(projects);
    }
    
    // --- POST /api/projects ---
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        var ownerId = GetUserId;
        var project = await _projectService.CreateProjectAsync(ownerId, dto);
        // Змінено на Created (201), оскільки GetProjectId очікує 201
        return CreatedAtAction(nameof(GetProjectId), new { projectId = project}, project); 
    }

    // --- GET /api/projects/{projectId} ---
    [HttpGet("{projectId}")]
    // Примітка: GetProjectId має повертати 200 Ok, а не 201 Created
    [ProducesResponseType(typeof(ProjectDetailsDto), 200)] 
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)] // Додано 404, якщо проект не знайдено
    public async Task<IActionResult> GetProjectId(Guid projectId)
    {
        var ownerId = GetUserId;
        // Припускаємо, що GetProjetDetailsAsync має логіку перевірки доступу
        var project = await _projectService.GetProjetDetailsAsync(projectId, ownerId); 
        return Ok(project);
    }
    
    // --- POST /api/projects/{projectId}/members ---
    [HttpPost("{projectId}/members")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid projectId, // <-- Повинен бути FromRoute
        [FromBody] AddMemberDto dto 
    )
    {
        var ownerId = GetUserId;
        await _projectService.AddMemberAsync(projectId, ownerId, dto);
        return NoContent();
    }
    
    // --- DELETE /api/projects/{projectId}/members/{memberId} ---
    [HttpDelete("{projectId}/members/{memberId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid projectId, // 👈 Явне вказання
        [FromRoute] Guid memberId   // 👈 Явне вказання
    )
    {
        var ownerId = GetUserId;
        await _projectService.RemoveMemberAsync(projectId, ownerId, memberId);
        return NoContent();
    } 

    // --- PUT /api/projects/{projectId} ---
    [HttpPut("{projectId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateProject(
        [FromRoute] Guid projectId, // 👈 Явне вказання
        [FromBody] UpdateProjectDto updateDto // 👈 Явне вказання
    )
    {
        var ownerId = GetUserId;
        await _projectService.UpdateProjectAsync(projectId, ownerId, updateDto);
        return NoContent();
    }
}