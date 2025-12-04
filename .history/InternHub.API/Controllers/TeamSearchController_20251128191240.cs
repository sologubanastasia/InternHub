namespace InternHub.API.Controllers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/teamsearch")]
public class TeamSearchController : ControllerBase
{
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        
    }

    [HttpPost("projects/{projectId}/apply")]
    public async Task<IActionResult> ApplyToProject(Guid projectId, [FromBody] ApplyToProjectDto dto)
    { 
        
    }

    [HttpGet("projects/{projectId}/requests")]
    public async Task<IActionResult> GetRequestsForProject(Guid projectId)
    {

    }
    
    [HttpPut("projects/{projectId}/requests/{requestId}/process")]
    public async Task<IActionResult> ProcessRequestForProject(Guid projectId, Guid requestId, [FromBody] ProcessRequestDto dto)
    {
        
    }
}