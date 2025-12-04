using InternHub.Application.DTO.Company;
using InternHub.Application.DTO.Job;
using InternHub.Application.Services.Company;
using InternHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Company")]
    [Route("api/company")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;  
        private readonly IUserContextService _userContext;
        public CompanyController(ICompanyService companyService, IUserContextService userContext)
        {
            _companyService = companyService;
            _userContext = _userContext;
        }
       

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _companyService.GetProfileAsync(GetUserId());
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCompanyDto dto)
        {
            await _companyService.UpdateProfileAsync(GetUserId(), dto);
            return NoContent();
        }

        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteProfile()
        {
            await _companyService.DeleteProfileAsync(GetUserId());
            return NoContent();
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetCompanyJobs()
        {
            var jobs = await _companyService.GetCompanyJobsAsync(GetUserId());
            return Ok(jobs);
        }

        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            await _companyService.CreateJobAsync(GetUserId(), dto);
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
        public async Task<IActionResult> UploadDocument([FromBody] CompanyDocumentUploadDto dto)
        {
            await _companyService.UploadDocumentAsync(GetUserId(), dto);
            return NoContent();
        }
    }
}
