namespace InternHub.Application.DTO.Auth
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
        public AuthResponseDto? Data { get; set; }
        
    }

}   