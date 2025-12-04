namespace InternHub.Application.DTO.Auth
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
        public AuthResponseDto? Data { get; set; }
    }

    public static AuthResultDto Ok(AuthResponseDto data) => new()
    {
        Success = true,
        StatusCode = 200,
        Data = data
    };

        public static AuthResultDto Unathirized(string error) => new()
        {
            Success = false,
            StatusCode = 401,
            Error = error
        };
    
    public 
}   