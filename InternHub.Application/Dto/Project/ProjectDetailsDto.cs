using InternHub.Application.DTO.Technology;
using InternHub.Application.DTO.Team;
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
        public string OwnerName { get; set; } = null!;
       public List<TechnologyDto> Technologies { get; set; } = new List<TechnologyDto>();
        public List<ProjectMemberDto> Members { get; set; } = new List<ProjectMemberDto>();
        public List<TeamRequestDetailsDto> TeamRequests { get; set; } = new List<TeamRequestDetailsDto>();
    }
}