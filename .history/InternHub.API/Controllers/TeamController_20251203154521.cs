namespace InternHub.API.Controllers;

using InternHub.Application.Services.Team;
using Microsoft.AspNetCore.Authorization;
using InternHub.Application.DTO.Team;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/team")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;
    
    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
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

    [HttpGet("projects")]
    [ProducesResponseType(typeof(IList<TeamProjectDto>), 200)]
    public async Task<IActionResult> GetProjects()
    {
        var project = await _teamService.GetTeamProjectsAsync();
        return Ok(project);
    }

    [HttpPost("projects/{projectId}/apply")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ApplyToProject(Guid projectId, [FromBody] ApplyToProjectDto dto)
    { 
        var userId = GetUserId;
        await _teamService.ApplyToProjectAsync(projectId, userId, dto);
        return NoContent();
    }

    [HttpGet("projects/{projectId}/requests")]
    [ProducesResponseType(typeof(IList<TeamRequestDetailsDto>), 200)]
    public async Task<IActionResult> GetRequestsForProject(Guid projectId)
    {
        var ownerId = GetUserId;
        var request = await _teamService.GetTeamRequestDetailsAsync(ownerId, projectId);
        return Ok(request);
    }
    
    [HttpPut("projects/{projectId}/requests/{requestId}/process")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ProcessRequestForProject(Guid projectId, Guid requestId, [FromBody] ProcessRequestDto dto)
    {
        var ownerId = GetUserId;
        await _teamService.RemoveMemberAsync(projectId, requestId, ownerId, dto);
        return NoContent();
    }
}