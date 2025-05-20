using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDMS.Core.Services;
using SDMS.Core.DTOs;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeesController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("startup/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(int startupId)
        {
            var employees = await _employeeService.GetEmployeesAsync(startupId);
            return Ok(employees);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,StartupFounder,Employee")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeService.UpdateEmployeeAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/trainings")]
        [Authorize(Roles = "Admin,StartupFounder,Employee")]
        public async Task<ActionResult<IEnumerable<TrainingDto>>> GetEmployeeTrainings(int id)
        {
            var trainings = await _employeeService.GetEmployeeTrainingsAsync(id);
            return Ok(trainings);
        }

        [HttpGet("trainings/{id}")]
        [Authorize(Roles = "Admin,StartupFounder,Employee")]
        public async Task<ActionResult<TrainingDto>> GetTraining(int id)
        {
            var training = await _employeeService.GetTrainingAsync(id);
            if (training == null)
                return NotFound();

            return Ok(training);
        }

        [HttpPost("trainings")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<TrainingDto>> CreateTraining(TrainingCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var training = await _employeeService.CreateTrainingAsync(dto);
            return CreatedAtAction(nameof(GetTraining), new { id = training.Id }, training);
        }

        [HttpPut("trainings/{id}")]
        [Authorize(Roles = "Admin,StartupFounder,Employee")]
        public async Task<IActionResult> UpdateTraining(int id, TrainingUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeService.UpdateTrainingAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}/performance")]
        [Authorize(Roles = "Admin,StartupFounder,Employee")]
        public async Task<ActionResult<PerformanceSummaryDto>> GetPerformanceSummary(int id)
        {
            var summary = await _employeeService.GetPerformanceSummaryAsync(id);
            if (summary == null)
                return NotFound();

            return Ok(summary);
        }

        [HttpGet("team-performance/{startupId}")]
        [Authorize(Roles = "Admin,StartupFounder")]
        public async Task<ActionResult<TeamPerformanceDto>> GetTeamPerformance(int startupId)
        {
            var performance = await _employeeService.GetTeamPerformanceAsync(startupId);
            return Ok(performance);
        }
    }
}
