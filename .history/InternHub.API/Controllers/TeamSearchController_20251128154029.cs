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

тепер підкажи як зробити автомапер  вірно якщо маємо такі дтошки так класи а логіку ти знаєш namespace InternHub.Application.DTO.Team

{

    public class TeamProjectDto

    {

        public Guid Id { get; set;}

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string OwnerName{ get; set;}

        public List<string> TechnologyNames { get; set;} = new List<string>();

        public int MemberCount{get; set;}

        public List<string> MemberNames {get; set;} = new List<string>();

    }

    public class MemberPreviewDto

    {

        public string FullName { get; set; } = null!;

        public string Role { get; set; } = null!; 

    }

}namespace InternHub.Application.DTO.Team

{

    public class ApplyToProjectDto

    {

        public string? Message { get; set;}

    }

}using InternHub.Domain.Entities;



namespace InternHub.Application.DTO.Team

{

    public class ProcessRequestDto

    {

        public ApplicationStatus Action { get; set;}

        public string? Role { get; set;}

    }

}namespace InternHub.Application.DTO.Team

{

    public class TeamRequestDetailsDto

    {

        public Guid RequestId{get; set;}

        public Guid CandidateId { get; set;} 

        public string CandidateName { get; set; } = null!;

        public string? Message { get; set;}

        public DateTime RequestedAt { get; set;}

        public string Status { get; set;} = null!;

        public string GitHubUrl { get; set; } = null!;

    }

}namespace InternHub.Application.DTO.Project

{

    public class ProjectMemberDto

    {

        public Guid CandidateId {get; set;}

        public string Name { get; set; } = null!;

        public string Role {get; set;} = null!;

        public string? GitHubUrl { get; set; }

    }

}namespace InternHub.Application.DTO.Project

{

    public class ProjectListItemDto

    {

        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public bool IsComplited { get; set;} = false;

        public bool IsTeamSearchActive { get; set; } = true;

    }

}namespace InternHub.Application.DTO.Project

{

    public class ProjectDetailsDto

    {

        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? RepositoryLink { get; set;}

        public string? DemoVideoUrl { get; set;}

        public bool IsComplited { get; set;} = false;

        public bool IsTeamSearchActive { get; set; } = true;

        public List<string> TechnologiesName { get; set; } = new List<string>();

        public List<ProjectMemberDto> ProjectMembers { get; set;}  = new List<ProjectMemberDto>();   

    }

}namespace InternHub.Application.DTO.Project

{

    public class CreateProjectDto

    {

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? RepositoryLink { get; set;}

        public string? DemoVideoUrl { get; set;}

        public bool IsTeamSearchActive { get; set; } = true;

        public List<Guid> ProjectTechnologies { get; set; } = new List<Guid>();

    }

}namespace InternHub.Application.DTO.Project

{

    public class AddMemberDto

    {

        public Guid CandidateId {get; set;}

        public string Role {get; set;} = null!;

    }

}namespace InternHub.Domain.Entities;

using System.Collections.Generic;

public class Project

{

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid CandidateId{ get; set;}

    public Candidate Candidate { get; set;} = null!;

    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? RepositoryLink { get; set;}

    public string? DemoVideoUrl { get; set;}

    public bool IsComplited { get; set;} = false;

    public bool IsTeamSearchActive { get; set; } = true;

    public ICollection<ProjectTechnology> ProjectTechnologies { get; set; } = new List<ProjectTechnology>();

    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    public ICollection<TeamRequest> TeamRequests { get; set; } = new List<TeamRequest>();

}using Microsoft.AspNetCore.Identity;



namespace InternHub.Domain.Entities;



public class ProjectMember

{

    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Project Project{get; set;} = null!;

    public Guid CandidateId { get; set;}

    public Candidate Candidate { get; set;} = null!;

    public string Role { get; set; } = null!;

}namespace InternHub.Domain.Entities;



public class ProjectTechnology

{

    public Guid ProjectId {get;set;}

    public Project Project{ get;set;} = null!;

    public Guid TechnologyId {get;set;}

    public Technology Technology{ get;set;} = null!;

}     namespace InternHub.Domain.Entities;



public class TeamRequest

{

    public Guid Id {get; set;}

    public Guid ProjectId { get; set;}

    public Project Project { get; set;} = null!;

    public Guid CandidateId {get;set;}

    public Candidate Candidate { get; set;} = null!;

    public DateTime RequestDate { get; set;} = DateTime.UtcNow;

    public ApplicationStatus Status { get; set;} = ApplicationStatus.Pending;

    public string? Message { get; set;}

}            namespace InternHub.Domain.Entities;



public class Technology

{

    public Guid Id{get;set;}

    public string Name{ get; set;} = null!;

    public ICollection<ProjectTechnology> ProjectTechnologies{get; set;} = new List<ProjectTechnology>();   

}         для чих методів namespace InternHub.API.Controllers;

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

    public async Task<IActionResult> CrфeateProject([FromBody] CreateProjectDto dto)

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