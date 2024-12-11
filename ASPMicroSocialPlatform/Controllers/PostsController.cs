using ASPMicroSocialPlatform.Data;
using ASPMicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace ASPMicroSocialPlatform.Controllers
{
    public class PostsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            // get all posts and include the user that posted them
            var posts = _context.Posts.Include(p => p.User).ToList();
            return View(posts);
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            // include comments and the user that posted the post and the users that posted the comments
            var post = _context.Posts
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);
            return View(post);
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> NewAsync(Post post, IFormFile Image)
        {
            post.Date = DateTime.Now;

            if (Image != null && Image.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    //return View(post);
                    // Need to handle the errors
                    return View();
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images",Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;
                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.Image));
                post.Image = databaseFileName;
            }

            post.UserId = _userManager.GetUserId(User);
            _context.Posts.Add(post);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

    }
}
