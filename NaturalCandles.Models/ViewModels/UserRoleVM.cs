namespace NaturalCandles.Models.ViewModels
{
    public class UserRoleVM
    {
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string CurrentRole { get; set; }
    }
}