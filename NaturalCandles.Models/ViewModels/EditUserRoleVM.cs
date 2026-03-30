using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NaturalCandles.Models.ViewModels
{
    public class EditUserRoleVM
    {
        public string UserId { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? CurrentRole { get; set; }

        [Required]
        public string NewRole { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}