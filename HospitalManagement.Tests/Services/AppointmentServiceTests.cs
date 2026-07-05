using FluentAssertions;
using HospitalManagement.Application.DTOs.Appointment;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Services;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using Moq;

namespace HospitalManagement.Tests.Services
{
    public class AppointmentServiceTests
    {
        // These are FAKE (mocked) dependencies
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
        private readonly Mock<IDoctorRepository> _mockDoctorRepo;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly Mock<IAuditLogRepository> _mockAuditRepo;

        // This is the REAL service we are testing
        private readonly AppointmentService _service;

        // Runs before EVERY test — fresh mocks each time
        public AppointmentServiceTests()
        {
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockAuditRepo = new Mock<IAuditLogRepository>();

            // Real service with FAKE repositories injected
            _service = new AppointmentService(
                _mockAppointmentRepo.Object,
                _mockDoctorRepo.Object,
                _mockPatientRepo.Object,
                _mockAuditRepo.Object);
        }

        // =====================================================
        // TEST 1 — Booking succeeds when slot is available
        // =====================================================
        [Fact]
        public async Task BookAppointment_ShouldSucceed_WhenSlotIsAvailable()
        {
            // ARRANGE — set up fake data
            var patientUserId = 1;
            var doctorId = 1;
            var appointmentDate = DateTime.UtcNow.AddDays(1);

            var fakePatient = new Patient
            {
                Id = 1,
                UserId = patientUserId,
                User = new User { Id = patientUserId, FullName = "Test Patient" }
            };

            var fakeDoctor = new Doctor
            {
                Id = doctorId,
                MaxDailyAppointments = 10,
                User = new User { FullName = "Dr. Test" },
                Specialization = "General"
            };

            var fakeCreatedAppointment = new Appointment
            {
                Id = 1,
                DoctorId = doctorId,
                PatientId = 1,
                AppointmentDate = appointmentDate,
                Reason = "Fever",
                Status = AppointmentStatus.Requested,
                Doctor = fakeDoctor,
                Patient = fakePatient
            };

            // Tell fake repos what to return when called
            _mockPatientRepo
                .Setup(r => r.GetByUserIdAsync(patientUserId))
                .ReturnsAsync(fakePatient);

            _mockDoctorRepo
                .Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(fakeDoctor);

            _mockAppointmentRepo
                .Setup(r => r.IsSlotTakenAsync(doctorId, appointmentDate))
                .ReturnsAsync(false);  // slot is FREE

            _mockAppointmentRepo
                .Setup(r => r.GetDoctorAppointmentCountForDateAsync(
                    doctorId, appointmentDate.Date))
                .ReturnsAsync(0);  // no appointments yet today

            _mockAppointmentRepo
                .Setup(r => r.CreateAsync(It.IsAny<Appointment>()))
                .ReturnsAsync(fakeCreatedAppointment);

            _mockAuditRepo
                .Setup(r => r.LogAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var request = new BookAppointmentRequest
            {
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                Reason = "Fever"
            };

            // ACT — call the real method
            var result = await _service.BookAppointmentAsync(request, patientUserId);

            // ASSERT — verify the result
            result.Should().NotBeNull();
            result.Status.Should().Be("Requested");
            result.DoctorName.Should().Be("Dr. Test");

            // Verify CreateAsync was called exactly once
            _mockAppointmentRepo.Verify(
                r => r.CreateAsync(It.IsAny<Appointment>()), Times.Once);
        }

        // =====================================================
        // TEST 2 — Booking fails when slot is already taken
        // =====================================================
        [Fact]
        public async Task BookAppointment_ShouldFail_WhenSlotIsAlreadyTaken()
        {
            // ARRANGE
            var doctorId = 1;
            var appointmentDate = DateTime.UtcNow.AddDays(1);

            _mockPatientRepo
                .Setup(r => r.GetByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Patient { Id = 1, UserId = 1 });

            _mockDoctorRepo
                .Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(new Doctor
                {
                    Id = doctorId,
                    MaxDailyAppointments = 10
                });

            // Slot IS taken — key difference from Test 1
            _mockAppointmentRepo
                .Setup(r => r.IsSlotTakenAsync(doctorId, appointmentDate))
                .ReturnsAsync(true);

            var request = new BookAppointmentRequest
            {
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                Reason = "Checkup"
            };

            // ACT + ASSERT — expect exception
            var act = async () =>
                await _service.BookAppointmentAsync(request, 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*slot*already booked*");

            // Verify database was NEVER called to create
            _mockAppointmentRepo.Verify(
                r => r.CreateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        // =====================================================
        // TEST 3 — Booking fails when date is in the past
        // =====================================================
        [Fact]
        public async Task BookAppointment_ShouldFail_WhenDateIsInThePast()
        {
            // ARRANGE
            _mockPatientRepo
                .Setup(r => r.GetByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Patient { Id = 1, UserId = 1 });

            _mockDoctorRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Doctor
                {
                    Id = 1,
                    MaxDailyAppointments = 10
                });

            var request = new BookAppointmentRequest
            {
                DoctorId = 1,
                AppointmentDate = DateTime.UtcNow.AddDays(-1), // PAST date
                Reason = "Back pain"
            };

            // ACT + ASSERT
            var act = async () =>
                await _service.BookAppointmentAsync(request, 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*future*");
        }

        // =====================================================
        // TEST 4 — Booking fails when doctor hits daily limit
        // =====================================================
        [Fact]
        public async Task BookAppointment_ShouldFail_WhenDoctorReachedDailyLimit()
        {
            // ARRANGE
            var doctorId = 1;
            var appointmentDate = DateTime.UtcNow.AddDays(1);

            _mockPatientRepo
                .Setup(r => r.GetByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Patient { Id = 1, UserId = 1 });

            _mockDoctorRepo
                .Setup(r => r.GetByIdAsync(doctorId))
                .ReturnsAsync(new Doctor
                {
                    Id = doctorId,
                    MaxDailyAppointments = 5  // max is 5
                });

            _mockAppointmentRepo
                .Setup(r => r.IsSlotTakenAsync(doctorId, appointmentDate))
                .ReturnsAsync(false);

            // Doctor already has 5 today — at max
            _mockAppointmentRepo
                .Setup(r => r.GetDoctorAppointmentCountForDateAsync(
                    doctorId, appointmentDate.Date))
                .ReturnsAsync(5);

            var request = new BookAppointmentRequest
            {
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                Reason = "Annual checkup"
            };

            // ACT + ASSERT
            var act = async () =>
                await _service.BookAppointmentAsync(request, 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*maximum*");
        }

        // =====================================================
        // TEST 5 — Booking fails when patient not found
        // =====================================================
        [Fact]
        public async Task BookAppointment_ShouldFail_WhenPatientNotFound()
        {
            // ARRANGE — return null (patient doesn't exist)
            _mockPatientRepo
                .Setup(r => r.GetByUserIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Patient?)null);

            var request = new BookAppointmentRequest
            {
                DoctorId = 1,
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Reason = "Checkup"
            };

            // ACT + ASSERT
            var act = async () =>
                await _service.BookAppointmentAsync(request, 99);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Patient profile*not found*");
        }

        // =====================================================
        // TEST 6 — Status update succeeds for valid transition
        // =====================================================
        [Fact]
        public async Task UpdateStatus_ShouldSucceed_WhenTransitionIsValid()
        {
            // ARRANGE
            var doctor = new Doctor
            {
                Id = 1,
                Specialization = "General",
                User = new User { FullName = "Dr. Test" }
            };

            var patient = new Patient
            {
                Id = 1,
                User = new User { FullName = "Patient Test" }
            };

            var existingAppointment = new Appointment
            {
                Id = 1,
                Status = AppointmentStatus.Requested, // current status
                DoctorId = 1,
                PatientId = 1,
                Reason = "Test",
                Doctor = doctor,
                Patient = patient
            };

            _mockAppointmentRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingAppointment);

            _mockAppointmentRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);

            _mockAuditRepo
                .Setup(r => r.LogAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var request = new UpdateAppointmentStatusRequest
            {
                NewStatus = AppointmentStatus.Confirmed // valid: Requested → Confirmed
            };

            // ACT
            var result = await _service.UpdateStatusAsync(1, request, 1, "Doctor");

            // ASSERT
            result.Should().NotBeNull();
            result.Status.Should().Be("Confirmed");

            _mockAppointmentRepo.Verify(
                r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Once);
        }

        // =====================================================
        // TEST 7 — Status update fails for invalid transition
        // =====================================================
        [Fact]
        public async Task UpdateStatus_ShouldFail_WhenTransitionIsInvalid()
        {
            // ARRANGE — Requested → Completed is not allowed (must go via Confirmed)
            var existingAppointment = new Appointment
            {
                Id = 1,
                Status = AppointmentStatus.Requested,
                Doctor = new Doctor
                {
                    User = new User { FullName = "Dr. Test" },
                    Specialization = "General"
                },
                Patient = new Patient
                {
                    User = new User { FullName = "Patient Test" }
                }
            };

            _mockAppointmentRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingAppointment);

            var request = new UpdateAppointmentStatusRequest
            {
                NewStatus = AppointmentStatus.Completed // INVALID jump
            };

            // ACT + ASSERT
            var act = async () =>
                await _service.UpdateStatusAsync(1, request, 1, "Doctor");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Cannot change*");

            // Verify database was never updated
            _mockAppointmentRepo.Verify(
                r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        // =====================================================
        // TEST 8 — GetAll returns appointments correctly
        // =====================================================
        [Fact]
        public async Task GetAll_ShouldReturnPagedAppointments()
        {
            // ARRANGE
            var fakeAppointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = 1,
                    Reason = "Fever",
                    Status = AppointmentStatus.Requested,
                    AppointmentDate = DateTime.UtcNow.AddDays(1),
                    Doctor = new Doctor
                    {
                        User = new User { FullName = "Dr. A" },
                        Specialization = "General"
                    },
                    Patient = new Patient
                    {
                        User = new User { FullName = "Patient A" }
                    }
                },
                new Appointment
                {
                    Id = 2,
                    Reason = "Checkup",
                    Status = AppointmentStatus.Confirmed,
                    AppointmentDate = DateTime.UtcNow.AddDays(2),
                    Doctor = new Doctor
                    {
                        User = new User { FullName = "Dr. B" },
                        Specialization = "Cardiology"
                    },
                    Patient = new Patient
                    {
                        User = new User { FullName = "Patient B" }
                    }
                }
            };

            _mockAppointmentRepo
                .Setup(r => r.GetAllAsync(1, 10))
                .ReturnsAsync(fakeAppointments);

            // ACT
            var result = (await _service.GetAllAsync(1, 10)).ToList();

            // ASSERT
            result.Should().HaveCount(2);
            result[0].DoctorName.Should().Be("Dr. A");
            result[1].Status.Should().Be("Confirmed");
        }
    }
}