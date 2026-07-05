using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.DTOs.Auth
{
    // This is what the caller sends when registering.
    // DataAnnotations validate the input automatically.
    public class RegisterRequest
    {
        [System.ComponentModel.DataAnnotations.Required]        // [Required] is a Data Annotation : a validation rule. 
        // When the API receives this DTO, ASP.NET Core automatically checks if FullName is present. If it's missing or empty, it returns a 400 Bad Request error automatically
        public string FullName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]    // [EmailAddress] validates that the value looks like a real email — contains @, has a domain, etc. 
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(6)]    //  Pw length must be atleast 6 char
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public UserRole Role { get; set; }

        // Extra info required only if registering as a Doctor since an Admin or Patient doesn't have these.
        public string? Specialization { get; set; }     // We validate this manually in AuthService with a proper error message instead of using [Required]       
        public string? LicenseNumber { get; set; }      // because [Required] would reject ALL registrations, not just doctors.

        // Extra info required only if registering as a Patient
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? BloodGroup { get; set; }
    }
}
