using ASPMicroSocialPlatform.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class GroupEditModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public List<string> SelectedUserIds { get; set; } = new List<string>();
    public List<ApplicationUser> SelectedUsers { get; set; } = new List<ApplicationUser>();
    public bool IsCurrentUserModerator { get; set; }
    public string CurrentUserId { get; set; }
}