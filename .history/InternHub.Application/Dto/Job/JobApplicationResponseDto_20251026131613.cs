using InternHub.Domain;

namespace InternHub.Application.DTO.Job
{
    public class JobApplicationResponseDto
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
