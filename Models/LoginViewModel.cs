using System.ComponentModel.DataAnnotations;

namespace ITTicketingSystem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember this device")]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);
    }
}
