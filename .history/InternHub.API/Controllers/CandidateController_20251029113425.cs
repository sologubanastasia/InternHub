using InternHub.Application.DTO.Candidate;
using InternHub.Application.Services.Candidate;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        public CandidateController(ICandidateService candidateService) => _candidateService = candidateService;

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId not found in claims"));

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _candidateService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateDto dto)
        {
            var userId = GetUserId();
            await _candidateService.UpdateProfileAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = GetUserId();
            await _candidateService.DeleteProfileAsync(userId);
            return NoContent();
        }

        [HttpPost("resume")]
        public async Task<IActionResult> UpdateResume([FromBody] UploadResumeDto dto)
        {
            var userId = GetUserId();
            await _candidateService.UpdateResumeAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete("resume")]
        public async Task<IActionResult> DeleteResume()
        {
            var userId = GetUserId();
            await _candidateService.DeleteResumeAsync(userId);
            return NoContent();
        }

        [HttpPost("video")]
        public async Task<IActionResult> UpdateVideo([FromBody] UploadVideoDto dto)
        {
            var userId = GetUserId();
            await _candidateService.UpdateVideoAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete("video")]
        public async Task<IActionResult> DeleteVideo()
        {
            var userId = GetUserId();
            await _candidateService.DeleteVideoAsync(userId);
            return NoContent();
        }
    }
}
