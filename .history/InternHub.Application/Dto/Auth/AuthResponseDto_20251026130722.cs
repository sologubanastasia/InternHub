namespace InternHub.Application.DTO.Auth
{
    public class AuthResponseDto
    {
        public Guid ApplicationUserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
