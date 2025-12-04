using AutoMapper;
using InternHub.Application.DTO.Auth;
using InternHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InternHub.Infrastructure;

namespace InternHub.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly InternHubDbContext _context;
        private readonly IMapper _mapper;

        public AuthService(InternHubDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            // 🧠 Автоматичний мапінг DTO -> ApplicationUser
            var user = _mapper.Map<ApplicationUser>(dto);

            // Ініціалізація додаткових зв’язків
            user.Id = Guid.NewGuid();
            user.UserName = dto.Email;
            user.PasswordHash = dto.Password; // TODO: hash password

            user.Candidate = _mapper.Map<Candidate>(dto);
            user.Candidate.Id = Guid.NewGuid();
            user.Candidate.Email = dto.Email;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Додати роль Candidate
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
            if (role == null)
                throw new Exception("Role 'Candidate' not found");

            _context.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = user.Id,
                RoleId = role.Id
            });

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = "Candidate",
                Token = "mocked-jwt-token"
            };
        }

        public async Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            var user = _mapper.Map<ApplicationUser>(dto);
            user.Id = Guid.NewGuid();
            user.UserName = dto.Email;
            user.PasswordHash = dto.Password;

            user.Company = _mapper.Map<Company>(dto);
            user.Company.Id = Guid.NewGuid();
            user.Company.Status = CompanyStatus.WaitingForAdminApproval;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Company");
            if (role == null)
                throw new Exception("Role 'Company' not found");

            _context.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = user.Id,
                RoleId = role.Id
            });

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = "Company",
                Token = "mocked-jwt-token"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Candidate)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.PasswordHash != dto.Password) // TODO: hash check
                throw new Exception("Invalid credentials.");

            var roleId = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .FirstOrDefaultAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            if (role?.Name == "Company" && user.Company?.Status == CompanyStatus.WaitingForAdminApproval)
                throw new Exception("Company not approved yet.");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = role?.Name ?? "Unknown",
                Token = "mocked-jwt-token"
            };
        }
    }
}
