 namespace InternHub.Application.DTO.Company
{
    public class CreateJobDto
    {
        public string Title { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}