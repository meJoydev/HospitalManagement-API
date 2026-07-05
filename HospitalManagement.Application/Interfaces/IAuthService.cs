using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Application.DTOs.Auth;

namespace HospitalManagement.Application.Interfaces
{
    public interface IAuthService       // IAuthService = contains business logic (validation, hashing, token generation)
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}