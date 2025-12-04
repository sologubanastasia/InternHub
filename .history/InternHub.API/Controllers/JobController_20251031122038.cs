using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IUserContextService _userContext;
        public JobController(IJobService jobService, IUserContextService _userContext)
        {
            _jobService = jobService;
            _userContext = _userContext;
        }

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

        [Authorize(Roles = "Candidate")]
        [HttpPost("{id}/apply")]
        public async Task<IActionResult> ApplyToJob(Guid id, [FromBody] JobApplicationCreateDto dto)
        {
            var candidateUserId = GetUserId();
            await _jobService.ApplyToJobAsync(id, candidateUserId, dto);
            return NoContent();
        }
    }
}
