namespace InternHub.Application.DTO.Project
{
    public class ProjectMemberDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId {get; set;}
        public string Name { get; set; } = null!;
        public string Role {get; set;} = null!;
        
        public Guid UserId{ get; set; }
        public string UserName{ get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}