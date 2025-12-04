using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<IEnumerable<ApplicationApplicationUser>> GetAllApplicationUsersAsync();
        Task<ApplicationApplicationUser?> GetApplicationUserByIdAsync(Guid ApplicationUserId);
        Task DeleteApplicationUserAsync(ApplicationApplicationUser ApplicationUser);
        Task<ApplicationApplicationUser?> GetApplicationUserByEmailAsync(string email);
        Task RegisterApplicationUserAsync(ApplicationApplicationUser ApplicationUser);
        Task<bool> ValidateApplicationUserCredentialAsync(string email, string password);
    }
}
