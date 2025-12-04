namespace InternHub.API.Controllers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    [HttpGet("my")]
    public async Task<IActionResult> GetProjects()
    {
        
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {

    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProjectId(Guid projectId)
    {
        
    }

    // [HttpPut{"projectId"}]
    // [HttpDelete{"projectId"}]

    [HttpPost("{projectId}/members")]
    public async Task<IActionResult> AddMember([FromBody] AddMemberDto dto)
    {
        
    }
    
    [HttpDelete("{projectId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid memberId)
    {
        
    } 
}
