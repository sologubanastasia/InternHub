namespace InternHub.Infrastructure.Services
{
    public interface IUserContextService
    {
        Guid GetUserId();
        string? GetUserRole();
    }
}