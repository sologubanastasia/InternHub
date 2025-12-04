namespace InternHub.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InternHub.Application.DTO.Pagination; 
using InternHub.Application.DTO.Technology; 
using InternHub.Application.Services.Technology; 

[Authorize]
[ApiController]
[Route("api/technology")]
public class TechnologyController : ControllerBase
{
    private readonly ITechnologyService _technologyService;
    public  TechnologyController(ITechnologyService technologyService)
    {
        _technologyService = technologyService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateTechnologyDto), 201)]
    public async Task<ActionResult> CreateTechnology([FromBody] CreateTechnologyDto dto)
    {
        var technology = await _technologyService.CreateTechnologyAsync(dto);
         return Ok(technology);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveTechnology(Guid id)
    {
        await _technologyService.DeleteByIdTechnologyAsync(id);
        return NoContent();
    }

   [HttpDelete("{id}")]
    public async Task<IActionResult> GetByIdTechnology(Guid id)
    {
        var technology = await _technologyService.GetByIdTechnologyAsync(id);
        return Ok(technology);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResultDto<TechnologyDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTechnologyAsync([FromQuery] TechnologySearchQueryDto query)
    {
        if(query.PageNumber < 1 || query.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be positive");
        }

        var pagedResult = await _technologyService.SearchTechnologyAsync(query);
        return Ok(pagedResult);
    }
}
