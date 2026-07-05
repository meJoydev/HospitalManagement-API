using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<Appointment>> GetAllAsync(int page, int pageSize);     //  (this +Task<int> GetTotalCountAsync()) These two work together for pagination
        // instead of returning ALL appointments at once, it returns them in pages 
        Task<int> GetTotalCountAsync();     // returns total no. of appointments
        Task<bool> IsSlotTakenAsync(int doctorId, DateTime appointmentDate);    // The double-booking prevention check. Before creating any appointment
        Task<int> GetDoctorAppointmentCountForDateAsync(int doctorId, DateTime date);
        Task<Appointment> CreateAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
    }
}
