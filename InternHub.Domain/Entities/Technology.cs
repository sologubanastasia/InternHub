namespace InternHub.Domain.Entities;

public class Technology
{
    public Guid Id{get;set;}
    public string Name{ get; set;} = null!;
    public ICollection<ProjectTechnology> ProjectTechnologies{get; set;} = new List<ProjectTechnology>();   
}