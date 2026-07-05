using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.Application.DTOs.Auth
{
    // What we send BACK after successful register/login
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;     // Token — the JWT token the caller must include in every future request
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }     // ExpiresAt — when the token expires so the frontend knows when to re-login
    }
}