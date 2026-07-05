using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Application.DTOs.Appointment;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Services
{
    public class AppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IAuditLogRepository auditLogRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<AppointmentDto> BookAppointmentAsync(BookAppointmentRequest request, int patientUserId)
        {
            // VALIDATION CHECKS :
            // 1. Get the patient profile from the userId in the JWT token
            var patient = await _patientRepository.GetByUserIdAsync(patientUserId)      // Fetches Patient (Full object) from jwt token
                ?? throw new InvalidOperationException("Patient profile not found.");

            // 2. Check doctor exists
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId)         // Fetches Doctor (Full object) from jwt token
                ?? throw new InvalidOperationException("Doctor not found.");

            // 3. Validate appointment is in the future
            if (request.AppointmentDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Appointment date must be in the future.");

            // 4. Check if that exact slot is already taken (concurrency protection)
            var isSlotTaken = await _appointmentRepository.IsSlotTakenAsync(
                request.DoctorId, request.AppointmentDate);

            if (isSlotTaken)
                throw new InvalidOperationException("This time slot is already booked. Please choose another time.");

            // 5. Check doctor hasn't exceeded daily appointment limit
            var dailyCount = await _appointmentRepository.GetDoctorAppointmentCountForDateAsync(
                request.DoctorId, request.AppointmentDate.Date);

            if (dailyCount >= doctor.MaxDailyAppointments)
                throw new InvalidOperationException($"Doctor has reached maximum appointments ({doctor.MaxDailyAppointments}) for this day.");

            // 6. Create the appointment
            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,                // just id is attached, not the full information / doctor object
                PatientId = patient.Id,                     // just id is attached, not the full information / patient object
                AppointmentDate = request.AppointmentDate,
                Reason = request.Reason,
                Status = AppointmentStatus.Requested        //  every new appointment always starts in the "Requested" state
            };

            var created = await _appointmentRepository.CreateAsync(appointment);

            // 7. Log this action to the audit log
            // "Write a note in the hospital's permanent record book: who booked what, with whom, and when." This is the audit trail, useful for accountability later.
            await _auditLogRepository.LogAsync(
                "AppointmentBooked",
                "Appointment",
                created.Id,
                patientUserId,
                $"Patient booked appointment with Dr. {doctor.User?.FullName} on {request.AppointmentDate:yyyy-MM-dd HH:mm}");

            // Converts the raw database entity into a clean DTO (no rowversion no navigation properties, only what the user needs to see) before sending back to the user
            // Why need to pass doctor and patient seperately ? We never attached the full Doctor or Patient objects to this new appointment
            // Since we already have the full objects with names loaded, we simply reuse them instead of fetching everything again from the database
            return MapToDto(created, doctor, patient); 
        }

        public async Task<AppointmentDto> UpdateStatusAsync(int appointmentId, UpdateAppointmentStatusRequest request, int userId, string userRole)
        {
            // GetByIdAsync loads the appointment along with its Doctor and Patient using .Include() 
            // it means appointment.Doctor and appointment.Patient are already filled in this time 
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId)
                ?? throw new InvalidOperationException("Appointment not found.");

            // STATE MACHINE — only valid transitions are allowed
            // Requested → Confirmed (allowed), Requested → Completed : this method throws an exception and everything below this line never runs
            ValidateStatusTransition(appointment.Status, request.NewStatus, userRole);

            appointment.Status = request.NewStatus;
            appointment.Notes = request.Notes ?? appointment.Notes;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);      // Add Updated Appointment in Db

            await _auditLogRepository.LogAsync(             // Write a permanent note in the hospital's record book about this change.
                $"AppointmentStatus→{request.NewStatus}",
                "Appointment",
                appointment.Id,
                userId,
                $"Status changed from {appointment.Status} to {request.NewStatus}");
            // appointment.Doctor and appointment.Patient are directly loaded from the appointment object itself, not separate variables like in BookAppointmentAsync
            // But passing that exact same object as a separate argument, just because MapToDto requires it in that shape.
            return MapToDto(
                appointment,
                appointment.Doctor,
                appointment.Patient);
        }

        // Validates that status transitions are legal
        private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus next, string role)
        {
            var allowed = (current, next) switch
            {
                // Requested → Confirmed: only Doctor or Admin can do this
                (AppointmentStatus.Requested, AppointmentStatus.Confirmed) => true,
                // Requested → Cancelled: anyone can cancel a pending request
                (AppointmentStatus.Requested, AppointmentStatus.Cancelled) => true,
                // Confirmed → Completed: only Doctor or Admin
                (AppointmentStatus.Confirmed, AppointmentStatus.Completed) => true,
                // Confirmed → Cancelled: anyone
                (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
                // Everything else is invalid
                _ => false      // _ => false is the catch-all : "any combination not explicitly listed above is not allowed."
            };

            if (!allowed)
                throw new InvalidOperationException(
                    $"Cannot change appointment status from {current} to {next}.");
        }

        // Converts a domain entity to a DTO — clean data for the API response
        private static AppointmentDto MapToDto(Appointment a, Doctor? doctor, Patient? patient)
        {
            return new AppointmentDto
            {
                Id = a.Id,
                DoctorName = doctor?.User?.FullName ?? "Unknown",
                DoctorSpecialization = doctor?.Specialization ?? "Unknown",
                PatientName = patient?.User?.FullName ?? "Unknown",
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Notes = a.Notes,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt
            };
        }
        // All three methods fetch and convert appointments to DTOs — the only difference is the scope of data each one is allowed to access
        // Fetch a page of appointments from the database, then convert each one from Entity to DTO.
        // .Select(a => MapToDto(a, a.Doctor, a.Patient)) — this is LINQ. It loops through every appointment in the collection and runs MapToDto on each one
        public async Task<IEnumerable<AppointmentDto>> GetAllAsync(int page, int pageSize)      // Admin sees every appointment in the hospital
        {
            var appointments = await _appointmentRepository.GetAllAsync(page, pageSize);        // Pagination
            return appointments.Select(a => MapToDto(a, a.Doctor, a.Patient));
        }
        // doctorUserId comes from the JWT token, The token only gives us the doctor's UserId (login ID), not their Doctor.Id (Doctors table ID).
        // So this method first converts UserId → Doctor.Id, then uses that to fetch the right appointments.
        public async Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(int doctorUserId)       // A Doctor sees ONLY their own patients' appointments
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(doctorUserId)
                ?? throw new InvalidOperationException("Doctor profile not found.");
            var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctor.Id);
            return appointments.Select(a => MapToDto(a, a.Doctor, a.Patient));
        }
        // Without .Select, you'd have to manually write a foreach loop to convert each item one by one — .Select does this in a single clean line.
        public async Task<IEnumerable<AppointmentDto>> GetByPatientAsync(int patientUserId)     // A Patient sees ONLY their own appointments
        {
            var patient = await _patientRepository.GetByUserIdAsync(patientUserId)
                ?? throw new InvalidOperationException("Patient profile not found.");
            var appointments = await _appointmentRepository.GetByPatientIdAsync(patient.Id);
            return appointments.Select(a => MapToDto(a, a.Doctor, a.Patient));
        }
    }
}