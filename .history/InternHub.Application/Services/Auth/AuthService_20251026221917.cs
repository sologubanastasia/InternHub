using InternHub.Application.DTO.Auth;
using InternHub.Domain;
using InternHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using InternHub.Infrastructure;

namespace InternHub.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly InternHubDbC _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("User with this email already exists.");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                PasswordHash = dto.Password, // TODO: hash password
                Candidate = new Candidate
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Surname = dto.Surname
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Додаємо роль Candidate
            var role = await _context.Roles.FirstAsync(r => r.Name == "Candidate");
            _context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
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
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("User with this email already exists.");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Company = new Company
                {
                    Id = Guid.NewGuid(),
                    CompanyName = dto.CompanyName,
                    Status = CompanyStatus.WaitingForAdminApproval
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Додаємо роль Company
            var role = await _context.Roles.FirstAsync(r => r.Name == "Company");
            _context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
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
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Company)
                .Include(u => u.Candidate)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("Invalid credentials.");

            var roleName = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            if (roleName == "Company" && user.Company?.Status == CompanyStatus.WaitingForAdminApproval)
                throw new Exception("Company not approved yet.");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = roleName,
                Token = "mocked-jwt-token"
            };
        }
    }
}
