using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(int id);     // Need : When anyone referencing a specific doctor

        // Why GetByUserIdAsync if we already have GetByIdAsync? Because there are two different IDs for a doctor
        // Doctor.Id : the ID in the Doctors table
        // Doctor.UserId : the ID in the Users table(their login account)
        // When a doctor logs in, the JWT token contains their UserId (from the Users table). To find their Doctor profile UserId is required
        Task<Doctor?> GetByUserIdAsync(int userId); // Need : When Doctor viewing their own data

        // IEnumerable<Doctor> means "a collection of multiple Doctor objects" like a list.
        // IEnumerable is the simplest collection type in C#, it just means "something you can loop through."
        // We use this instead of List<Doctor> because it's more flexible and the actual implementation can return any kind of collection.
        Task<IEnumerable<Doctor>> GetAllAsync();    // Task<Doctor> returns ONE doctor , Task<IEnumerable<Doctor>> returns MULTIPLE doctors(a collection)

        // in Task<Doctor> CreateAsync  it takes doctor obj and saves new data to the database for the first time and
        // returns the object back with the newly assigned ID that SQL Server generated, which we need immediately for the next step.
        // Process : Doctor register their details(exists in HTTP req.) -> Authservice create object in memory (userid, name...) -> CreateAsync is called ->
        // EF Core runs insert operation in DB -> SQL Server assigns ID -> EF Core updates our object with the new ID(ID, userid, name...) -> Returns obj with ID.
        Task<Doctor> CreateAsync(Doctor doctor);    // if there's no <doctor> then no doctor id is returned then we can't link doctor to user 
        Task UpdateAsync(Doctor doctor);    // Updates and returns just Task with nothing inside the angle brackets
        Task<IEnumerable<DoctorSchedule>> GetScheduleAsync(int doctorId);
        Task AddScheduleAsync(DoctorSchedule schedule);
    }
}