using System.ComponentModel.DataAnnotations;

namespace ASPMicroSocialPlatform.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int? GroupId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu!")]
        public string? Content { get; set; }

        public DateTime Timestamp { get; set; }

        public virtual Group? Group { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
