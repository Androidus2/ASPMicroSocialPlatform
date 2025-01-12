using System.ComponentModel.DataAnnotations.Schema;

namespace ASPMicroSocialPlatform.Models
{
    public class Like
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? PostId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Post? Post { get; set; }

    }
}
