using ASPMicroSocialPlatform.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


public class GroupCreateModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public List<string> SelectedUserIds { get; set; } = new List<string>();

    [NotMapped]
    public List<ApplicationUser> SelectedUsers { get; set; } = new List<ApplicationUser>();
}