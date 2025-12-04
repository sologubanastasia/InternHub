namespace InternHub.Application.Services.Technology
{
    public class ITechnologyService
    {
        public Task<Guid> CreateTechnologyAsync(CreateTechnologyDto dto);

    }
}