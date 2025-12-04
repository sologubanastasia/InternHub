using AutoMapper;
using InternHub.Domain.Entities;
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
        private readonly InternHubDbConteФxt _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUserEntity> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AuthService(
            InternHubDbContext context,
            IMapper mapper,
            UserManager<ApplicationUserEntity> userManager,
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
    // 1️⃣ Перевірка на null або пусті значення
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentException("Email is required.");

    if (string.IsNullOrWhiteSpace(dto.Password))
        throw new ArgumentException("Password is required.");

    if (string.IsNullOrWhiteSpace(dto.CompanyName))
        throw new ArgumentException("Company name is required.");

    // 2️⃣ Перевіряємо, чи вже існує користувач з таким email
    var existingUser = await _userManager.FindByEmailAsync(dto.Email);
    if (existingUser != null)
        throw new InvalidOperationException("User with this email already exists.");

    // 3️⃣ Створюємо користувача через AutoMapper
    var user = _mapper.Map<ApplicationUserEntity>(dto);

    // Додаємо сутність компанії
    user.Company = new CompanyEntity
    {
        Id = Guid.NewGuid(),
        CompanyName = dto.CompanyName.Trim(),
        Status = CompanyStatus.WaitingForAdminApproval,
        Email = dto.Email
    };

    // 4️⃣ Створюємо користувача в Identity
    var createResult = await _userManager.CreateAsync(user, dto.Password);
    if (!createResult.Succeeded)
    {
        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Failed to create user: {errors}");
    }

    // 5️⃣ Переконуємося, що роль 'Company' існує
    if (!await _roleManager.RoleExistsAsync(RoleConstants.Company))
    {
        var role = new IdentityRole<Guid>(RoleConstants.Company);
        var roleResult = await _roleManager.CreateAsync(role);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role 'Company': {errors}");
        }
    }

    // 6️⃣ Додаємо користувача до ролі
    var addRoleResult = await _userManager.AddToRoleAsync(user, RoleConstants.Company);
    if (!addRoleResult.Succeeded)
    {
        var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Failed to assign role 'Company': {errors}");
    }

    // 7️⃣ Зберігаємо зміни в БД (компанія зв’язана з користувачем)
    await _context.SaveChangesAsync();

    // 8️⃣ Формуємо відповідь
    return new AuthResponseDto
    {
        UserId = user.Id,
        Email = user.Email,
        Role = RoleConstants.Company,
        Token = "mocked-jwt-token"
    };
}



        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.Users
            .Include(u => u.Company)
            .Include(u => u.Candidate)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return AuthResultDto.Unauthorized("Invalid email or password.");

        var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
            return AuthResultDto.Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? "Unknown";

        if (roleName == "Company" &&
            user.Company?.Status == InternHub.Domain.Entities.CompanyStatus.WaitingForAdminApproval)
            return AuthResultDto.BadRequest("Company not approved yet.");

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = roleName,
            Token = "mocked-jwt-token"
        };

        return AuthResultDto.Ok(response);
    }


    }
}
