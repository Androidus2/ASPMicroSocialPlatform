using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPMicroSocialPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }

        public bool? IsPrivate { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Like>? Likes { get; set; }

        public virtual ICollection<UserGroup>? UserGroups { get; set; }

        public virtual ICollection<Follow>? Followers { get; set; }
        public virtual ICollection<Follow>? Following { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
