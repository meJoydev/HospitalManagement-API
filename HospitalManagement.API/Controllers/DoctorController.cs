using HospitalManagement.Application.DTOs.Doctor;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorController(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorRepository.GetAllAsync();

            var result = doctors.Select(d => new DoctorDto
            {
                Id = d.Id,
                FullName = d.User?.FullName ?? "",
                Email = d.User?.Email ?? "",
                Specialization = d.Specialization,
                PhoneNumber = d.PhoneNumber,
                MaxDailyAppointments = d.MaxDailyAppointments
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            var result = new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.User?.FullName ?? "",
                Email = doctor.User?.Email ?? "",
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                MaxDailyAppointments = doctor.MaxDailyAppointments
            };

            return Ok(result);
        }

        [HttpGet("{id}/schedule")]
        public async Task<IActionResult> GetSchedule(int id)
        {
            var schedules = await _doctorRepository.GetScheduleAsync(id);

            var result = schedules.Select(s => new
            {
                s.Id,
                DayOfWeek = s.DayOfWeek.ToString(),
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                s.IsAvailable
            });

            return Ok(result);
        }

        [HttpPost("{id}/schedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSchedule(int id, [FromBody] DoctorScheduleDto request)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found." });

            var schedule = new DoctorSchedule
            {
                DoctorId = id,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsAvailable = true
            };

            await _doctorRepository.AddScheduleAsync(schedule);
            return StatusCode(201, new { message = "Schedule added successfully." });
        }
    }
}
