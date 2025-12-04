namespace InternHub.Domain;

public class User 
{
    pub;
    public string Name {get; set;}
    public string Surname {get; set;}
    public string? Description {get; set;}
    public bool IsActive {get; set;} = true;
    public Candidate? Candidate {get;set;}
    public Company? Company {get; set;}
}