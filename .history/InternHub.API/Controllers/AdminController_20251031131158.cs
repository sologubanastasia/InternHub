using InternHub.Application.DTO.Admin;
using InternHub.Application.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace InternHub.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService) => _adminService = adminService;

        [HttpGet("users")]
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies(int pageNum = 1, int pageSize = 10)
        {
            var companies = await _adminService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpPut("companies/{id}/approve")]
        public async Task<IActionResult> ApproveCompany(Guid id, [FromBody] ApproveCompanyDto dto)
        {
            await _adminService.ApproveCompanyAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _adminService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetAllJobs(int pageNum = 1, int pageSize = 10)
        {
            var jobs = await _adminService.GetAllJobsAsync();
            return Ok(jobs);
        }

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(Guid id)
        {
            await _adminService.DeleteJobAsync(id);
            return NoContent();
        }
    }
}
