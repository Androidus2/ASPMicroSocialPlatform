using System.ComponentModel.DataAnnotations;

namespace ASPMicroSocialPlatform.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele grupului este obligatoriu!")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Descrierea grupului este obligatorie!")]
        public string? Description { get; set; }


        public virtual ICollection<Message>? Messages { get; set; }
        public virtual ICollection<UserGroup>? UserGroups { get; set; }
    }
}
