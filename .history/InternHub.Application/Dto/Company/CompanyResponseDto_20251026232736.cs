using InternHub.Domainю;
namespace InternHub.Application.DTO.Company
{
    public class CompanyResponseDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = CompanyStatus.WaitingForAdminApproval.ToString();
    }
}    