namespace InternHub.Domain.Entities;

public class Candidate
{
    public Guid Id{get;set;}
    public Guid UserId{ get; set;}
    public ApplicationUser? User { get; set; } 
    public string? GitHubUrl{ get; set;}
    public string? Email {get;set;}
    public string? Telegram{get; set;}
    public string? ResumeUrl{ get; set;}
    public string? VideoUrl{get; set;}
    public ICollection<Application> Applications{get; set;} = new List<Application>();
    public ICollection<Project> Projects{get; set;} = new List<Project>();
    public ICollection<TeamRequest> TeamRequests{get; set;} = new List<TeamRequest>();
}
