using InternHub.Domain;

namespace InternHub.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        
        Task<User> GetUserByEmailAsync(string email);
        Task RegisterUserAsync(User user);
        Task<bool> ValidateUserCredentialAsync(string email, string password);
    }
}    