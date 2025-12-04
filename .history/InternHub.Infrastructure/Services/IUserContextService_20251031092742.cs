namespace InternHub.Application.Services
{
    public interface IUserContextService
    {
        Guid GetUserId();
        string? GetUserRole();
    }
}