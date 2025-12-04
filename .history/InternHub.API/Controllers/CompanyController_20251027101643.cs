using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Company;
using Microsoft.AspNetCore.Mvc;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/company")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService) => _companyService = companyService;

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] Guid userId)
        {
            var profile = await _companyService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] Guid userId, [FromBody] UpdateCompanyDto dto)
        {
            await _companyService.UpdateProfileAsync(userId, dto);
            return NoContent();
        }

        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteProfile([FromQuery] Guid userId)
        {
            await _companyService.DeleteProfileAsync(userId);
            return NoContent();
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetCompanyJobs([FromQuery] Guid userId)
        {
            var jobs = await _companyService.GetCompanyJobsAsync(userId);
            return Ok(jobs);
        }

        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromQuery] Guid userId, [FromBody] CreateJobDto dto)
        {
            await _companyService.CreateJobAsync(userId, dto);
            return NoContent();
        }

        [HttpGet("jobs/{id}/applications")]
        public async Task<IActionResult> GetJobApplications(Guid id)
        {
            var applications = await _companyService.GetJobApplicationsAsync(id);
            return Ok(applications);
        }

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(Guid id)
        {
            await _companyService.DeleteJobAsync(id);
            return NoContent();
        }

        [HttpPost("documents")]
        public async Task<IActionResult> UploadDocument([FromQuery] Guid userId, [FromBody] CompanyDocumentUploadDto dto)
        {
            await _companyService.UploadDocumentAsync(userId, dto);
            return NoContent();
        }
    }
}
