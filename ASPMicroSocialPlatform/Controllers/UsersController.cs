using ASPMicroSocialPlatform.Data;
using ASPMicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
		[Authorize(Roles= "User,Admin")]
		public IActionResult Index()
		{
			var users = _context.Users.ToList();
			return View(users);
		}

		[HttpGet]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Show(string id)
		{
            // get the user and include all of their posts only if the user has a public profile or
            // we are an admin or the user is the same as the one in the session or the user is following the 
            // user whose profile we are trying to access

            // load the user without the posts
            var user = _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefault(u => u.Id == id);

            // check if the user is private
            if (user.IsPrivate == false || id == _userManager.GetUserId(User) || User.IsInRole("Admin") || user.Followers.Any(f => f.FollowerId == _userManager.GetUserId(User)))
            {
                user = _context.Users
                    .Include(u => u.Posts)
                    .Include(u => u.Followers)
                    .Include(u => u.Following)
                    .FirstOrDefault(u => u.Id == id);
                ViewBag.ShowPosts = true;
            }
            else
            {
                user = _context.Users
                    .FirstOrDefault(u => u.Id == id);
                ViewBag.ShowPosts = false;
            }

            return View(user);
		}

		[HttpGet]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Edit(string id)
		{
			if (id != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu ai permisiunea de a edita acest profil!";
                TempData["messageType"] = "error";
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
			return View(user);
		}

		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public async Task<IActionResult> EditAsync(ApplicationUser user, IFormFile? ProfilePicture, string? ExistingProfilePicture)
		{
            // check if the user is the same as the one in the session
            if (user.Id != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
			{
				TempData["message"] = "Nu ai permisiunea de a edita acest profil!";
				TempData["messageType"] = "error";
                return RedirectToAction("Index", "Home");
            }

			if(ProfilePicture != null)
            {
                // Verificam extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(ProfilePicture.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fisierul trebuie sa fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    TempData["message"] = "Imaginea nu a putut fi adaugata!";
                    TempData["messageType"] = "error";
                    return View(user);
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images", ProfilePicture.FileName);
                var databaseFileName = "/images/" + ProfilePicture.FileName;
                // Salvare fisier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(user.ProfilePicture));
                user.ProfilePicture = databaseFileName;
            }
            else
            {
                user.ProfilePicture = ExistingProfilePicture;
            }

            var userInDb = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            userInDb.FirstName = user.FirstName;
            userInDb.LastName = user.LastName;
            userInDb.Bio = user.Bio;
            userInDb.ProfilePicture = user.ProfilePicture;
            userInDb.IsPrivate = user.IsPrivate;

            _context.SaveChanges();

            TempData["message"] = "Profilul a fost actualizat!";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", new { id = user.Id });
		}

	}
}
