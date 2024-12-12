using System.ComponentModel.DataAnnotations;

namespace ASPMicroSocialPlatform.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        public int? PostId { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu!")]
        public string? Content { get; set; }

		public DateTime Date { get; set; }

		public virtual ApplicationUser? User { get; set; }

        public virtual Post? Post { get; set; }
    }
}
