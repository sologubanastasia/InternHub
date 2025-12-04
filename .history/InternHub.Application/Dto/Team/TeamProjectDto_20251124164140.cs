namespace InternHub.Application.DTO.Team
{
    public class TeamProjectDto
    {
        public Guid Id { get; set;}
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string OwnerName{ get; set;}
        public List<string> TechnologyNames { get; set;} = new List<string>();
        public int MemberCount{get; set;}
        public List<string> MemberNames {get; set;} = new List<string>();
    }
    public class MemberPreviewDto
    {
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!; 
    }
}