namespace InternHub.Application.DTO.Company
{
    public class CompanyJobListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ApplicantsCount { get; set; }
    }
}
