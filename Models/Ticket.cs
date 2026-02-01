using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITTicketingSystem.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open";

        [StringLength(50)]
        public string? Department { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? WorkLocation { get; set; }

        [StringLength(100)]
        public string? TeamViewerId { get; set; }

        [StringLength(100)]
        public string? TeamViewerPassword { get; set; }

        [StringLength(50)]
        public string? ContactNumber { get; set; }

        [StringLength(255)]
        public string? ContactEmail { get; set; }

        [StringLength(255)]
        public string? Attachments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public int CreatedById { get; set; }

        public int? AssignedToId { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; } = null!;

        [ForeignKey("AssignedToId")]
        public virtual User? AssignedTo { get; set; }

        [NotMapped]
        public string TicketNumber => $"TK-{Id:D4}";

        [NotMapped]
        public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy - HH:mm");
    }
}
