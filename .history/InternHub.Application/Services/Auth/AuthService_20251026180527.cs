using InternHub.Application.DTO.Auth;
using InternHub.Application.Interfaces;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;

namespace InternHub.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            var existing = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = dto.Password, // TODO: hash password
                Role = UserRole.Candidate,
                Candidate = new Candidate
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Surname = dto.Surname
                }
            };

            await _authRepository.RegisterUserAsync(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = "mocked-jwt-token"
            };
        }

        public async Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto)
        {
            var existing = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = dto.Password,
                Role = UserRole.Company,
                Company = new Company
                {
                    Id = Guid.NewGuid(),
                    CompanyName = dto.CompanyName,
                    Status = CompanyStatus.WaitingForAdminApproval
                }
            };

            await _authRepository.RegisterUserAsync(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = "mocked-jwt-token"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid credentials.");

            if (user.Role == UserRole.Company && user.Company?.Status == CompanyStatus.WaitingForAdminApproval)
                throw new Exception("Company not approved yet.");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = "mocked-jwt-token"
            };
        }
    }
}
