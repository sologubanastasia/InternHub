using InternHub.Application.DTO.Candidate;
using InternHub.Application.Services.Candidate;
using InternHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        public CandidateController(ICandidateService candidateService) => _candidateService = candidateService;

        public CandidateController(ICandidateService candidateService, IUserContextService userContextService)
        {
            _candidateService = candidateService;
            _userContextService = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _candidateService.GetProfileAsync(GetUserId());
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateDto dto)
        {
            await _candidateService.UpdateProfileAsync(GetUserId(), dto);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            await _candidateService.DeleteProfileAsync(GetUserId());
            return NoContent();
        }

        [HttpPost("resume")]
        public async Task<IActionResult> UpdateResume([FromBody] UploadResumeDto dto)
        {
            await _candidateService.UpdateResumeAsync(GetUserId(), dto);
            return NoContent();
        }

        [HttpDelete("resume")]
        public async Task<IActionResult> DeleteResume()
        {
            await _candidateService.DeleteResumeAsync(GetUserId());
            return NoContent();
        }

        [HttpPost("video")]
        public async Task<IActionResult> UpdateVideo([FromBody] UploadVideoDto dto)
        {
            await _candidateService.UpdateVideoAsync(GetUserId(), dto);
            return NoContent();
        }

        [HttpDelete("video")]
        public async Task<IActionResult> DeleteVideo()
        {
            await _candidateService.DeleteVideoAsync(GetUserId());
            return NoContent();
        }
    }
}
