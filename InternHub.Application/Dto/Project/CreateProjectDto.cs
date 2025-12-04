namespace InternHub.Application.DTO.Project
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? RepositoryLink { get; set;}
        public string? DemoVideoUrl { get; set;}
        public bool IsTeamSearchActive { get; set; } = true;
        public List<Guid> ProjectTechnologies { get; set; } = new List<Guid>();
    }
}