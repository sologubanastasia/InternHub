namespace InternHub.Domain;
using System.Collections.Generic;

public class Company
{
    public Guid Id { get; set;}
    public Guid UserId{ get; set;}
    public string CompanyName{get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string? Website {get; set;}
    public string? Description {get; set;}

    public CompanyStatus Status {get; set;} = CompanyStatus.WaitingForAdminApproval;
    
    public ICollection<Job> Jobs{get; set;} = new List<Job>();
    public ICollection<CompanyDocument> Documents {get; set;} = new List<CompanyDocument>();
}

public enum CompanyStatus
{
    WaitingForAdminApproval,
    Approved,
    NotApproved
}