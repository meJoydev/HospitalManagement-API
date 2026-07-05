using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Patient>> GetAllAsync();       // Returns a colllection of patients
        Task<Patient> CreateAsync(Patient patient);     // It must return patient type object which includes newly generated patient_id by the SQL server
        Task UpdateAsync(Patient patient);
    }
}