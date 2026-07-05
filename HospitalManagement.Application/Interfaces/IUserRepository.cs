using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Interfaces
{
    public interface IUserRepository        // IUserRepository = talks to the database
    {
        // Task<> : this method is asynchronous.
        // User? : this method returns a User object. The ? means it might return null
        // if no user is found with that ID, it returns NULL instead of crashing
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);  // Used during login to look up the user.

        // Takes a User object as input and returns the created User back(now with an Id assigned by the database)
        Task<User> CreateAsync(User user);  // Always returns user and not Null
        Task<bool> EmailExistsAsync(string email);  // returns T/F if the email already exist in the db or not
    }
}
