using ASPMicroSocialPlatform.Data;
using ASPMicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ASPMicroSocialPlatform.Controllers
{
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public FollowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        // Send a follow request
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SendFollowRequest(string followedId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser.Id == followedId)
            {
                return BadRequest("You cannot follow yourself.");
            }

            // Check if a follow request or follow already exists
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowedId == followedId);

            if (existingFollow != null)
            {
                return BadRequest("You have already sent a follow request to this user.");
            }

            var followedUser = await _userManager.FindByIdAsync(followedId);
            if (followedUser == null)
            {
                return NotFound("User not found.");
            }

            var follow = new Follow
            {
                FollowerId = currentUser.Id,
                FollowedId = followedId,
                IsAccepted = !followedUser.IsPrivate.GetValueOrDefault(false) // Auto-accept if user is public
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", "Users", new { id = followedId });
        }

        // Accept a follow request
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> AcceptFollowRequest(string followerId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var followRequest = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == currentUser.Id && !f.IsAccepted);

            if (followRequest == null)
            {
                return NotFound("Follow request not found.");
            }

            followRequest.IsAccepted = true;
            _context.Follows.Update(followRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction("FollowRequests"); // !!!
        }

        // Reject a follow request
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> RejectFollowRequest(string followerId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            Console.WriteLine("ID" + followerId);

            var followRequest = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == currentUser.Id && !f.IsAccepted);

            if (followRequest == null)
            {
                return NotFound("Follow request not found.");
            }

            _context.Follows.Remove(followRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction("FollowRequests");
        }

        // View followers
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Followers(string userId)
        {
            Console.WriteLine("ID" + userId);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var followers = await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == userId && f.IsAccepted)
                .Select(f => f.Follower)
                .ToListAsync();



            if (userId != _userManager.GetUserId(User))
            {
                ViewBag.isSelf = false;
            }
            else
            {
                ViewBag.isSelf = true;
            }

            return View(followers); 
        }

        // View following
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Following(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var following = await _context.Follows
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == userId && f.IsAccepted)
                .Select(f => f.Followed)
                .ToListAsync();

            if (userId != _userManager.GetUserId(User))
            {
                ViewBag.isSelf = false;
            }
            else
            {
                ViewBag.isSelf = true;
            }

            return View(following); 
        }

        // Unfollow a user
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Unfollow(string followedId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            Console.WriteLine("id" + followedId, currentUser.Id);

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowedId == followedId);

            if (follow == null)
            {
                return NotFound("You are not following this user.");
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return RedirectToAction("Following", new { userId = currentUser.Id });
        }

        // Remove a follower
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> RemoveFollower(string followerId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            Console.WriteLine("id" + followerId);

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == currentUser.Id);

            if (follow == null)
            {
                return NotFound("The user is not following you.");
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return RedirectToAction("Followers", new { userId = currentUser.Id });
        }

        // View pending follow requests received by the current user
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> FollowRequests()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var followRequests = await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == currentUser.Id && !f.IsAccepted)
                .ToListAsync();

            return View(followRequests);
        }

    }



}

