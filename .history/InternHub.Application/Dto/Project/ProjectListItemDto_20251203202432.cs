namespace InternHub.Application.DTO.Project
{
    public class ProjectListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsComplited { get; set;} = false;
        public bool IsTeamSearchActive { get; set; } = true;
        public List<string> TechnologyNames { get; set; } = new List<string>(); 
        public string OwnerName { get; set; } = null!;
    }
}