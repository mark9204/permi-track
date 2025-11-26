using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermiTrack.DataContext.DTOs
{
   public class CreateUserRequest
    {
        public string Username { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string Password { get; set; } = default!; // Changed from PasswordHash - will be hashed in service

        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }

        public bool IsActive { get; set; } = true;

        public bool EmailVerified { get; set; } = false; // Admin can create pre-verified users
    }
}
