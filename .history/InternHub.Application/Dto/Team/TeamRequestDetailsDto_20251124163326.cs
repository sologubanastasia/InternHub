namespace InternHub.Application.DTO.Team
{
    public class TeamRequestDetailsDto
    {
        public Guid RequestId{get; set;}
        public Guid CandidateId { get; set;} 
        public string CandidateName { get; set; } = null!;
        public string? Message { get; set;}
        public DateTime RequestedAt { get; set;}
        public string Status { get; set;} = null!;
        public string GitHubUrl { get; set; } = null!;
    }
}