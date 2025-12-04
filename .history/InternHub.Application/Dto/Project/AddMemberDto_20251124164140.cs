namespace InternHub.Application.DTO.Project
{
    public class AddMemberDto
    {
        public Guid CandidateId {get; set;}
        public string Role {get; set;} = null!;
    }
}