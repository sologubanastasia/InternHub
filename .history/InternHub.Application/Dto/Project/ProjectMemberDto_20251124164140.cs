namespace InternHub.Application.DTO.Project
{
    public class ProjectMemberDto
    {
        public Guid CandidateId {get; set;}
        public string Name { get; set; } = null!;
        public string Role {get; set;} = null!;
        public string? GitHubUrl { get; set; }
    }
}