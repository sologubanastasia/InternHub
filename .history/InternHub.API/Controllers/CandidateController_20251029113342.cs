using InternHub.Application.DTO.Candidate;
using InternHub.Application.Services.Candidate;
using Microsoft.AspNetCore.Mvc;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        public CandidateController(ICandidateService candidateService) => _candidateService = candidateService;

        [HttpGet]
Й        public async Task<IActionResult> GetProfile([FromQuery] Guid userId)
        {
            var profile = await _candidateService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromQuery] Guid userId, [FromBody] UpdateCandidateDto dto)
        {
            await _candidateService.UpdateProfileAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfile([FromQuery] Guid userId)
        {
            await _candidateService.DeleteProfileAsync(userId);
            return NoContent();
        }

        [HttpPost("resume")]
        public async Task<IActionResult> UpdateResume([FromQuery] Guid userId, [FromBody] UploadResumeDto dto)
        {
            await _candidateService.UpdateResumeAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete("resume")]
        public async Task<IActionResult> DeleteResume([FromQuery] Guid userId)
        {
            await _candidateService.DeleteResumeAsync(userId);
            return NoContent();
        }

        [HttpPost("video")]
        public async Task<IActionResult> UpdateVideo([FromQuery] Guid userId, [FromBody] UploadVideoDto dto)
        {
            await _candidateService.UpdateVideoAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete("video")]
        public async Task<IActionResult> DeleteVideo([FromQuery] Guid userId)
        {
            await _candidateService.DeleteVideoAsync(userId);
            return NoContent();
        }
    }
}
