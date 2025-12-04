using InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        
        Task RegisterUserAsync(ApplicationUser user, string password, string roleName);
        
        Task<bool> ValidateUserCredentialAsync(string email, string password);
        Task DeleteUserAsync(ApplicationUser user);
    }
}
