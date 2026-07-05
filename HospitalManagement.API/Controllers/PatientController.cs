using HospitalManagement.Application.DTOs.Patient;
using HospitalManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;

        public PatientController(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _patientRepository.GetAllAsync();

            var result = patients.Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.User?.FullName ?? "",
                Email = p.User?.Email ?? "",
                PhoneNumber = p.PhoneNumber,
                Gender = p.Gender,
                DateOfBirth = p.DateOfBirth,
                BloodGroup = p.BloodGroup,
                MedicalHistory = p.MedicalHistory
            });

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var patient = await _patientRepository.GetByUserIdAsync(userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found." });

            return Ok(new PatientDto
            {
                Id = patient.Id,
                FullName = patient.User?.FullName ?? "",
                Email = patient.User?.Email ?? "",
                PhoneNumber = patient.PhoneNumber,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                BloodGroup = patient.BloodGroup,
                MedicalHistory = patient.MedicalHistory
            });
        }
    }
}
