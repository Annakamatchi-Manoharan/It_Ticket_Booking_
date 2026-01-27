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

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Issue Category")]
        public string? Category { get; set; }

        [Display(Name = "Attachments")]
        public List<IFormFile>? Attachments { get; set; } = new List<IFormFile>();

        public string? ErrorMessage { get; set; }

        public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);
    }
}
