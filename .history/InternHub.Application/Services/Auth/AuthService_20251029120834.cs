using AutoMapper;
using InternHub.Application.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InternHub.Infrastructure;
using CandidateEntity = InternHub.Domain.Entities.Candidate;
using CompanyEntity = InternHub.Domain.Entities.Company;
using ApplicationUserEntity = InternHub.Domain.Entities.ApplicationUser;

namespace InternHub.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly InternHubDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AuthService(
            InternHubDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            var user = _mapper.Map<ApplicationUserEntity>(dto);
            user.Candidate = new CandidateEntity
            {
                Id = Guid.NewGuid(),
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("Candidate"))
                throw new Exception("Role 'Candidate' not found");

            await _userManager.AddToRoleAsync(user, "Candidate");

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
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new Exception("User with this email already exists.");

            var user = _mapper.Map<ApplicationUserEntity>(dto);
            user.Company = new CompanyEntity
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.CompanyName,
                Status = InternHub.Domain.Entities.CompanyStatus.WaitingForAdminApproval
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("Company"))
                throw new Exception("Role 'Company' not found");

            await _userManager.AddToRoleAsync(user, "Company");

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
            var user = await _userManager.Users
                .Include(u => u.Company)
                .Include(u => u.Candidate)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid credentials.");

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? "Unknown";

            if (roleName == "Company" && user.Company?.Status == InternHub.Domain.Entities.CompanyStatus.WaitingForAdminApproval)
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
