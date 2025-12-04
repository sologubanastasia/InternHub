using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(Guid userId);
        Task DeleteUserAsync(ApplicationUser user);
        Task<User> GetUserByEmailAsync(string email);
        Task RegisterUserAsync(User user);
        Task<bool> ValidateUserCredentialAsync(string email, string password);
    }
}    