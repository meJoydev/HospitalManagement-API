using FluentAssertions;
using HospitalManagement.Application.DTOs.Auth;
using HospitalManagement.Application.Interfaces;
using HospitalManagement.Application.Services;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HospitalManagement.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IDoctorRepository> _mockDoctorRepo;
        private readonly Mock<IPatientRepository> _mockPatientRepo;
        private readonly IConfiguration _configuration;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();

            // Build a fake IConfiguration with JWT settings
            // This avoids needing a real appsettings.json in tests
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:SecretKey", "TestSecretKeyThatIsAtLeast32CharactersLong!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _service = new AuthService(
                _mockUserRepo.Object,
                _mockDoctorRepo.Object,
                _mockPatientRepo.Object,
                _configuration);
        }

        // =====================================================
        // TEST 1 — Login succeeds with correct credentials
        // =====================================================
        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // ARRANGE
            var password = "Test@123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var fakeUser = new User
            {
                Id = 1,
                FullName = "Test User",
                Email = "test@test.com",
                PasswordHash = hashedPassword,
                Role = UserRole.Patient,
                IsActive = true
            };

            _mockUserRepo
                .Setup(r => r.GetByEmailAsync("test@test.com"))
                .ReturnsAsync(fakeUser);

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = password
            };

            // ACT
            var result = await _service.LoginAsync(request);

            // ASSERT
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be("test@test.com");
            result.Role.Should().Be("Patient");
        }

        // =====================================================
        // TEST 2 — Login fails with wrong password
        // =====================================================
        [Fact]
        public async Task Login_ShouldFail_WhenPasswordIsWrong()
        {
            // ARRANGE
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

            _mockUserRepo
                .Setup(r => r.GetByEmailAsync("test@test.com"))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    Email = "test@test.com",
                    PasswordHash = hashedPassword,
                    IsActive = true,
                    Role = UserRole.Patient
                });

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "WrongPassword"  // wrong password
            };

            // ACT + ASSERT
            var act = async () => await _service.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid email or password*");
        }

        // =====================================================
        // TEST 3 — Login fails when email doesn't exist
        // =====================================================
        [Fact]
        public async Task Login_ShouldFail_WhenEmailDoesNotExist()
        {
            // ARRANGE — return null (user not found)
            _mockUserRepo
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var request = new LoginRequest
            {
                Email = "nobody@test.com",
                Password = "SomePassword"
            };

            // ACT + ASSERT
            var act = async () => await _service.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid email or password*");
        }

        // =====================================================
        // TEST 4 — Registration fails when email already exists
        // =====================================================
        [Fact]
        public async Task Register_ShouldFail_WhenEmailAlreadyExists()
        {
            // ARRANGE — email already taken
            _mockUserRepo
                .Setup(r => r.EmailExistsAsync("existing@test.com"))
                .ReturnsAsync(true);

            var request = new RegisterRequest
            {
                FullName = "New User",
                Email = "existing@test.com",
                Password = "Password@123",
                Role = UserRole.Patient,
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                Gender = "Male"
            };

            // ACT + ASSERT
            var act = async () => await _service.RegisterAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already registered*");
        }

        // =====================================================
        // TEST 5 — Login fails when account is deactivated
        // =====================================================
        [Fact]
        public async Task Login_ShouldFail_WhenAccountIsDeactivated()
        {
            // ARRANGE
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@123");

            _mockUserRepo
                .Setup(r => r.GetByEmailAsync("inactive@test.com"))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    Email = "inactive@test.com",
                    PasswordHash = hashedPassword,
                    IsActive = false,  // account deactivated
                    Role = UserRole.Patient
                });

            var request = new LoginRequest
            {
                Email = "inactive@test.com",
                Password = "Test@123"
            };

            // ACT + ASSERT
            var act = async () => await _service.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*deactivated*");
        }
    }
}