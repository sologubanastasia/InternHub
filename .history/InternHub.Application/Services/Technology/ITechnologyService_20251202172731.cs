using InternHub.Application.DTO.Technology;
using InternHub.Application.DTO.Pagination;
namespace InternHub.Application.Services.Technology
{
    public interface ITechnologyService
    {  
        Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyDto dto); 
        Task<TechnologyDto> GetByIdTechnologyAsync(Guid id);
        Task DeleteByIdTechnologyAsync(Guid id);
        Task<PagedResultDto<TechnologyDto>> SearchTechnologyAsync(TechnologySearchQueryDto query);
    }
}