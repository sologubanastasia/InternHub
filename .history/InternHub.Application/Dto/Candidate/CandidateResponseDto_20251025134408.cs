namespace InternHub.Application.DTO
{
    public class CandidateResponseDto
    {
        public Guid Id { get; set; }
        public string? GitHubUrl { get; set; }
        public string? Email { get; set; }
        public string? Telegram { get; set; }
        public string? ResumeUrl { get; set; }
        public string? VideoUrl { get; set; }
    }
}
