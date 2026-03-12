using System.ComponentModel.DataAnnotations;

namespace ITTicketingSystem.Models
{
    public class TeamOverviewViewModel
    {
        public int? EditUserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Name")]
        [StringLength(200, ErrorMessage = "Name must be less than 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirm password must match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User";

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Name")]
        [StringLength(200, ErrorMessage = "Name must be less than 200 characters.")]
        public string EditName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email")]
        public string EditEmail { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string EditPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(EditPassword), ErrorMessage = "Password and confirm password must match.")]
        [Display(Name = "Confirm Password")]
        public string EditConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        public string EditRole { get; set; } = "User";

        public IReadOnlyList<User> Users { get; set; } = Array.Empty<User>();
    }
}
