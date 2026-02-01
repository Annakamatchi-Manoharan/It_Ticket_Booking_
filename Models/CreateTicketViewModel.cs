using System.ComponentModel.DataAnnotations;

namespace ITTicketingSystem.Models
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "Ticket subject is required")]
        [StringLength(255, ErrorMessage = "Subject cannot exceed 255 characters")]
        [Display(Name = "Ticket Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Detailed Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority level is required")]
        [Display(Name = "Priority Level")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issue category is required")]
        [Display(Name = "Issue Category")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Attachments")]
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();

        [Required(ErrorMessage = "Work location is required")]
        public string WorkLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "TeamViewer ID is required")]
        public string TeamViewerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "TeamViewer password is required")]
        public string TeamViewerPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Please enter a valid contact number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string ContactEmail { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);
    }
}
