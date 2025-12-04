using InternHub.Application.DTO.Auth;
using InternHub.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InternHub.Infrastructure;
namespace InternHub.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly InternHubDbContext _context;

        public AuthService(InternHubDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            // Перевірка на існуючого користувача
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            // Створення користувача
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                PasswordHash = dto.Password, // TODO: замінити на хешування
                Candidate = new Candidate
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Surname = dto.Surname
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Додати роль Candidate
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
            if (role == null)
                throw new Exception("Role 'Candidate' not found in database.");

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

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                PasswordHash = dto.Password, // TODO: хешування
                Company = new Company
                {
                    Id = Guid.NewGuid(),
                    CompanyName = dto.CompanyName,
                    Status = CompanyStatus.WaitingForAdminApproval
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Додати роль Company
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Company");
            if (role == null)
                throw new Exception("Role 'Company' not found in database.");

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
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.PasswordHash != dto.Password) // TODO: замінити на перевірку хешу
                throw new Exception("Invalid credentials.");

            // Отримати роль користувача
            var roleId = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .FirstOrDefaultAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            if (role.Name == "Company" && user.Company?.Status == CompanyStatus.WaitingForAdminApproval)
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
