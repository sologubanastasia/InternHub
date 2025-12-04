namespace InternHub.Domain.Entities;
using System.Collections.Generic;

public class Job
{
    public Guid Id {get; set;}
    public Guid CompanyId{get; set;}
    public Company Company {get; set; } = null!;
    public string Title {get; set;} 
    public string Requirements {get; set;} 
    public string? Location {get; set;}
    public bool IsActive {get; set;} = true;
    public DateTime CreatedDate{ get; set;} = DateTime.UtcNow;

    public ICollection<Application> Applications {get; set;} = new List<Application>();
}