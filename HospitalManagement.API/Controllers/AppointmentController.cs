using HospitalManagement.Application.DTOs.Appointment;
using HospitalManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentController(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(claim!);
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 50)
                return BadRequest(new { message = "Invalid pagination parameters." });

            var appointments = await _appointmentService.GetAllAsync(page, pageSize);
            return Ok(new { page, pageSize, data = appointments });
        }

        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var appointments = await _appointmentService
                .GetByPatientAsync(GetCurrentUserId());
            return Ok(appointments);
        }

        [HttpGet("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var appointments = await _appointmentService
                .GetByDoctorAsync(GetCurrentUserId());
            return Ok(appointments);
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> BookAppointment(
            [FromBody] BookAppointmentRequest request)
        {
            var result = await _appointmentService
                .BookAppointmentAsync(request, GetCurrentUserId());
            return StatusCode(201, result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateAppointmentStatusRequest request)
        {
            var result = await _appointmentService.UpdateStatusAsync(
                id,
                request,
                GetCurrentUserId(),
                GetCurrentUserRole());
            return Ok(result);
        }
    }
}
