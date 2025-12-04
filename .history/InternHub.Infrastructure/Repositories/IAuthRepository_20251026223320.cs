uusing InternHub.Domain.Entities;

namespace InternHub.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
        Task DeleteUserAsync(ApplicationUser user);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task RegisterUserAsync(ApplicationUser user);
        Task<bool> ValidateUserCredentialAsync(string email, string password);
    }
}
