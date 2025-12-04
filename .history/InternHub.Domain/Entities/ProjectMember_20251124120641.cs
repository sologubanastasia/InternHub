using Microsoft.AspNetCore.Identity;

namespace InternHub.Domain.Entities;

public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project{get; set;} = null!;
    public Guid CandidateId { get; set;}
    public Candidate Candidate { get; set;} = null!;
    public string Role { get; set; } = null!;
}