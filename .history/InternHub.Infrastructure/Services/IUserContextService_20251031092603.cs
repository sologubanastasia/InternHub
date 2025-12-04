namespace InternHub.Application.Interfaces
{
    public interface IUserContextService
    {
        Guid GetUserId();
        string? GetUserRole();
    }
}