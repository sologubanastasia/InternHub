namespace InternHub.Domain.Entities;

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
}