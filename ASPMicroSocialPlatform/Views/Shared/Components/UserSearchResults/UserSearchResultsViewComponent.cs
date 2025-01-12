using ASPMicroSocialPlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPMicroSocialPlatform.Models;

public class UserSearchResultsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public UserSearchResultsViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string searchTerm)
    {
        var users = string.IsNullOrEmpty(searchTerm) ? new List<ApplicationUser>() : await _context.Users
                .Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm) || u.Email.Contains(searchTerm))
                .ToListAsync();

        return View(users);
    }
}
   