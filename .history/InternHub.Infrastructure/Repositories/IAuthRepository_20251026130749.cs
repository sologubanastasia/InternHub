using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllApplicationUsersAsync();
        Task<ApplicationUser?> GetApplicationUserByIdAsync(Guid ApplicationUserId);
        Task DeleteApplicationUserAsync(ApplicationUser ApplicationUser);
        Task<ApplicationUser?> GetApplicationUserByEmailAsync(string email);
        Task RegisterApplicationUserAsync(ApplicationUser ApplicationUser);
        Task<bool> ValidateApplicationUserCredentialAsync(string email, string password);
    }
}
