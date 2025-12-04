using AutoMapper;
using TechnologyEntity = InternHub.Domain.Entities.Technology;
using InternHub.Application.DTO.Pagination;
using InternHub.Application.DTO.Technology;
using InternHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InternHub.Application.Services.Technology
{
    public class TechnologyService : ITechnologyService
    {
        private readonly ITechnologyRepository _technologyRepository;
        private readonly IMapper _mapper;
        public TechnologyService(ITechnologyRepository technologyRepository, IMapper mapper)
        {
            _technologyRepository = technologyRepository;
            _mapper = mapper;
        }
        public async Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyDto dto)
        {
            var name = await _technologyRepository.GetByNameAsync(dto.Name);

            if(name != null)
            {
                throw new InvalidOperationException($"Technology {dto.Name} already exist");
            }
            var technology = _mapper.Map<TechnologyEntity>(dto);

            await _technologyRepository.AddAsync(technology);
            await _technologyRepository.SaveChangesAsync();

            return _mapper.Map<TechnologyDto>(technology);
        }

        public async Task DeleteByIdTechnologyAsync(Guid id)
        {
            var technology = await _technologyRepository.GetByIdAsync(id);

            if(technology == null)
            {
                throw new KeyNotFoundException($"Technology with ID {id} not found");
            }

            await _technologyRepository.DeleteAsync(technology);
            await _technologyRepository.SaveChangesAsync();

        }

        public async Task<TechnologyDto> GetByIdTechnologyAsync(Guid id)
        {
            var technology = await _technologyRepository.GetByIdAsync(id);

            if(technology == null)
            {
                throw new KeyNotFoundException($"Technology with ID - {id} not found");
            }

            return _mapper.Map<TechnologyDto>(technology);
        }

        public async Task <PagedResultDto<TechnologyDto>> SearchTechnologyAsync(TechnologySearchQueryDto query)
        {
            var baseQuery = _technologyRepository.GetTechnologyQuery();

            if(!string.IsNullOrWhiteSpace(query.Name))
            {
                var searchName = query.Name.ToLower();
                baseQuery = baseQuery.Where(t => t.Name.ToLower().Contains(searchName));
            }

            baseQuery = query.SortDirection.ToLower() == "desc"
                ? baseQuery.OrderByDescending(t => t.Name)
                : baseQuery.OrderBy(t => t.Name);

            var totalCount = await baseQuery.CountAsync();

            var pagedQuery= baseQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            var technologies = await pagedQuery.ToListAsync();
            var technologyDtos= _mapper.Map<IList<TechnologyDto>>(technologies);

            return new PagedResultDto<TechnologyDto>
            {
                Items = technologyDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}