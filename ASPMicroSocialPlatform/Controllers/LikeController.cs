using ASPMicroSocialPlatform.Data;
using ASPMicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPMicroSocialPlatform.Controllers
{
    public class LikeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public LikeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        // Like a post
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                TempData["message"] = "Postarea nu exista!";
                TempData["messageType"] = "error";
                return RedirectToAction("Index", "Posts");
            }

            // Check if the user has already liked the post
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == currentUser.Id && l.PostId == postId);

            if (existingLike != null)
            {
                // Unlike the post
                _context.Likes.Remove(existingLike);

                await _context.SaveChangesAsync();

                TempData["message"] = "Ai dat unlike la postare!";
                TempData["messageType"] = "success";
                return Redirect("/Posts/Show/" + postId);
            }

            var like = new Like
            {
                UserId = currentUser.Id,
                PostId = postId
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            TempData["message"] = "Ai dat like la postare!";
            TempData["messageType"] = "success";
            return Redirect("/Posts/Show/" + postId);
        }


    }
}
