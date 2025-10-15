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

        public string PasswordHash { get; set; } = default!;        

        public string? FirstName { get; set; } 

        public string? LastName { get; set; }
    }
}
