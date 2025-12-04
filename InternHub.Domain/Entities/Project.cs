namespace InternHub.Domain.Entities;
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
}