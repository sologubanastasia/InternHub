using AutoMapper;
using InternHub.Application.DTO.Auth;
using InternHub.Domain.Entities;
using InternHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

using CandidateEntity = InternHub.Domain.Entities.Candidate;
using CompanyEntity = InternHub.Domain.Entities.Company;

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

        // ✅ Реєстрація кандидата
        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            // Мапінг DTO → ApplicationUser
            var user = _mapper.Map<ApplicationUser>(dto);

            // Створюємо сутність кандидата
            user.Candidate = new CandidateEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Surname = dto.Surname
            };

            // Тимчасово зберігаємо пароль без хешування (TODO: додай хешування)
            user.PasswordHash = dto.Password;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Додаємо роль
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
            if (role == null)
                throw new Exception("Role 'Candidate' not found.");

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

        // ✅ Реєстрація компанії
        public async Task<AuthResponseDto> RegisterCompanyAsync(RegisterCompanyDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            var user = _mapper.Map<ApplicationUser>(dto);

            user.Company = new CompanyEntity
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.CompanyName,
                Status = CompanyStatus.WaitingForAdminApproval
            };

            user.PasswordHash = dto.Password;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Company");
            if (role == null)
                throw new Exception("Role 'Company' not found.");

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

        // ✅ Логін користувача
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Candidate)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.PasswordHash != dto.Password)
                throw new Exception("Invalid credentials.");

            var roleId = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .FirstOrDefaultAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            if (role?.Name == "Company" &&
                user.Company?.Status == CompanyStatus.WaitingForAdminApproval)
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
