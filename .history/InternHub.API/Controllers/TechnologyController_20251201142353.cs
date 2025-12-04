namespace InternHub.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        var technologyId = await _technologyService.CreateTechnologyAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = technologyId}, technologyId);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllTechnologies()
    {
        var technologies = await _technologyService.GetAllTechnologiesAsync();
        return Ok(technologies);
    }

    [HttpGet("id")]
    public async Task<IActionResult> GetByIdTechnology(Guid id)
    {
        var technology = await _technologyService.GetByIdTechnologyAsync(id);
        return Ok(technology);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAsync([FromQuery] TechnologySearchQueryDto query)
    {
        if(query.PageNumber < 1 || query.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be positive");
        }

        var pagedResult = await _technologyService.SearchTechnologyAsunc(query);
    }
}
https://www.youtube.com/watch?v=dCVAYB2jkEY&list=PLpEoiloH-4eP-OKItF8XNJ8y8e1asOJud&index=13