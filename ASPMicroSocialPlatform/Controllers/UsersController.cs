using ASPMicroSocialPlatform.Data;
using ASPMicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPMicroSocialPlatform.Controllers
{
	public class UsersController : Controller
	{

		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IWebHostEnvironment _env;

		public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
		{
			_context = context;
			_userManager = userManager;
			_roleManager = roleManager;
			_env = env;
		}

		[HttpGet]
		[Authorize(Roles= "Admin")]
		public IActionResult Index()
		{
			var users = _context.Users.ToList();
			return View(users);
		}

		[HttpGet]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Show(string id)
		{
            // get the user and include all of their posts
            var user = _context.Users
                .Include(u => u.Posts)
                .FirstOrDefault(u => u.Id == id);
            return View(user);
		}

	}
}
