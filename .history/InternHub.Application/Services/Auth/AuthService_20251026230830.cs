using InternHub.Application.DTO.Auth;
using InternHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InternHub.Infrastructure;
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

        public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existing != null) throw new Exception("User with this email already exists.");

            // Мапінг DTO у ApplicationUser через AutoMapper
            var user = _mapper.Map<ApplicationUser>(dto);
            user.Id = Guid.NewGuid();
            user.Candidate.Id = Guid.NewGuid();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate");
            if (role == null) throw new Exception("Role 'Candidate' not found");

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
    }

}
