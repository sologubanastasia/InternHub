namespace InternHub.Application.DTO.Project
{
    public class ProjectMemberDto
    {
        public Guid CandidateId {get; set;}
        
        public string Role {get; set;} = null!;
    }
}