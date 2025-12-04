using InternHub.Domain.Entities;

namespace InternHub.Application.DTO.Team
{
    public class ProcessRequestDto
    {
        public ApplicationStatus Action { get; set;}
        public string? Role { get; set;}
    }
}