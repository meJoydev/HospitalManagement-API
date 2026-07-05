using HospitalManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HospitalManagement.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        // Foreign keys — which doctor and patient this appointment is for
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        // Doctor and Patient are navigation properties — shortcuts to get the full Doctor or Patient object without writing joins.
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }  // The date and time of the appointment
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }  // Doctor can add notes after the appointment

        // Uses our enum. Default value is Requested — every new appointment starts as Requested.
        // It can only move to Confirmed, Completed, or Cancelled through the state machine we build in AppointmentService.
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ? means nullable — when an appointment is first created, it has never been updated, so this is null.
        public DateTime? UpdatedAt { get; set; }

        // Concurrency token — prevents two users from modifying the same appointment simultaneously
        // Imagine two doctors open the same appointment at exactly the same time and both try to confirm it.Without this, both saves go through and you get duplicate confirmations
        // RowVersion is a special column that SQL Server automatically updates every time a row changes. When EF Core saves an appointment,
        // it checks: "is the RowVersion in the database still the same as when I loaded it?" If someone else changed it in between, EF Core throws a DbUpdateConcurrencyException
        // EF Core checks this value hasn't changed before saving
        // byte[] is just the data type SQL Server uses for this (an array of bytes)
        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}