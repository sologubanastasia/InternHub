namespace InternHub.Domain;
public class Candidate
{
    public Guid Id{get;set;}
    public Guid UserId{ get; set;}
    public User User {get; set;} = null!;
    public string? GitHubUrl{ get; set;}
    public string? Email {get;set;}
    public string? Telegram{get; set;}
    public string? ResumeUrl{ get; set;}
    public string? VideoUrl{get; set;}
    
    public ICollection<Application> Applications{get; ser;} = new List<Applicatiion>()
}
