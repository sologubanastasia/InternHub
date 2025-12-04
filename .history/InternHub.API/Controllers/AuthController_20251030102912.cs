using InternHub.Application.DTO.Auth;
using InternHub.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> RegisterCandidate([FromBody] RegisterCandidateDto dto)
        {
            var result = await _authService.RegisterCandidateAsync(dto);
            return Ok(result);
        }

        [HttpPost("register/company")]
        public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyDto dto)
        {
            var result = await _authService.RegisterCompanyAsync(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return StatusCode(result.StatusCode, result.Success
                ? result.Data
                : new { error = result.Error });
        }
    }
}
