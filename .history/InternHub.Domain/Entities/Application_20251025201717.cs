namespace InternHub.Domain;

public class Application
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? Message { get; set; }
}

public enum ApplicationStatus
{
    Pending,
    Accepted,
    Rejected
}
