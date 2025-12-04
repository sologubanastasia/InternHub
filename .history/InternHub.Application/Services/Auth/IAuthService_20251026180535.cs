using InternHub.Application.DTO.Auth;

namespace InternHub.Application.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto);
        Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
