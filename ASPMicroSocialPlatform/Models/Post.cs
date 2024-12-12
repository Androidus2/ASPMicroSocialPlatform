using System.ComponentModel.DataAnnotations;

namespace ASPMicroSocialPlatform.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie!")]
        public string? Description { get; set; }

        public string? Image { get; set; }
        // To do: multiple pictures per post

        public DateTime Date { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
