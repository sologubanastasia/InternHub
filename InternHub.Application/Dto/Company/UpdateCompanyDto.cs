namespace InternHub.Application.DTO.Company
{
    public class UpdateCompanyDto
    {
        public string CompanyName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Description { get; set; }
    }
}
