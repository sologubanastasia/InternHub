namespace InternHub.Application.DTO.Admin
{
    public class JobListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
