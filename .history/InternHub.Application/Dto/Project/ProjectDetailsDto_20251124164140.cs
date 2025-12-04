namespace InternHub.Application.DTO.Project
{
    public class ProjectDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? RepositoryLink { get; set;}
        public string? DemoVideoUrl { get; set;}
        public bool IsComplited { get; set;} = false;
        public bool IsTeamSearchActive { get; set; } = true;
        public List<string> TechnologiesName { get; set; } = new List<string>();
        public List<ProjectMemberDto> ProjectMembers { get; set;}  = new List<ProjectMemberDto>();   
    }
}