using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NaturalCandles.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}