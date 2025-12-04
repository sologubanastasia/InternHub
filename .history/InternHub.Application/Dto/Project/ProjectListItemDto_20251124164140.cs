namespace InternHub.Application.DTO.Project
{
    public class ProjectListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsComplited { get; set;} = false;
        public bool IsTeamSearchActive { get; set; } = true;
    }
}