using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure.Data
{
    public class AppDbContext : DbContext       // AppDbContext is the bridge that connects your C# classes to an actual database
    {
        // This is how AppDbContext receives its configuration, mainly the connection string (where the database lives)
        // DbContextOptions<AppDbContext> options : this carries settings like: "Use SQL Server", "Connect to this address: (localdb)\\mssqllocaldb", "Database name: HospitalManagementDB"
        // : base(options) : this passes those options up to the parent DbContext class so it knows how to actually connect.
        // The DI container creates it automatically and hands it these options, which come from appsettings.json via the DependencyInjection.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)     // Constructor Dependency Injection
        {
        }
        // Each line here says: "Create a table for this entity."
        // Without declaring DbSet<User> Users, EF Core wouldn't even know that User should become a database table at all.
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // This is where you write the detailed rules for each table like Which column is unique (no duplicates allowed), How tables connect to each other etc.
        // ModelBuilder modelBuilder : think of this as a pen that lets you write configuration rules onto the database blueprint.
        protected override void OnModelCreating(ModelBuilder modelBuilder)      // this method already exists in the parent DbContext class,
        {                                                                       // but it does nothing by default That's why we're overriding it to add our own custom rules.
            
            // This entire method only runs once — when EF Core first builds the database model (usually when you run a migration). It's not something that runs on every request.
            base.OnModelCreating(modelBuilder);     // always call this first. It lets the parent class do its own default setup before we add our custom rules on top.

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasConversion<int>();
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasOne(d => d.User)
                    .WithOne(u => u.Doctor)
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(d => d.Specialization).HasMaxLength(100).IsRequired();
                entity.Property(d => d.LicenseNumber).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Patient)
                    .HasForeignKey<Patient>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(p => p.Gender).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(a => a.Status).HasConversion<int>();
                entity.Property(a => a.Reason).HasMaxLength(500).IsRequired();
                entity.Property(a => a.RowVersion).IsRowVersion();
            });

            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.Doctor)
                    .WithMany(d => d.Schedules)
                    .HasForeignKey(s => s.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(s => s.DayOfWeek).HasConversion<int>();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
                entity.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Details).HasMaxLength(1000);
            });
        }
    }
}
