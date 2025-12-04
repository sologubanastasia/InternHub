namespace InternHub.Domain.Entities;

public class CompanyDocument
{
    public Guid Id{get;set;}
    public Guid CompanyId{get;set;}
    public Company Company {get; set;}= null!;
    public string FileName {get;set;} = string.Empty!;
    public string FileUrl {get;set;} = string.Empty!;
    public DateTime UploadDate { get; set;} = DateTime.UtcNow;
}

