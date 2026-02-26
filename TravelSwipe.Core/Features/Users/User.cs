using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Core.Features.Users
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Fullname { get; set; } = string.Empty;

        public string FacebookId { get; set; } = string.Empty;

        public string GoogleId { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;
    }
}
