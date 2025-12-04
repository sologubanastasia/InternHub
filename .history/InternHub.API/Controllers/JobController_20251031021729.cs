using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        public JobController(IJobService jobService) => _jobService = jobService;

        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(Guid id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            return Ok(job);
        }

        [HttpPost("{id}/apply")]
        public async Task<IActionResult> ApplyToJob(Guid id, [FromQuery] Guid candidateUserId, [FromBody] JobApplicationCreateDto dto)
        {
            await _jobService.ApplyToJobAsync(id, candidateUserId, dto);
            return NoContent();
        }
    }
}
