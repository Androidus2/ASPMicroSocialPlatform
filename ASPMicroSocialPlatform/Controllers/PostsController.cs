using ASPMicroSocialPlatform.Data;
using ASPMicroSocialPlatform.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
			// get all posts and include the user that posted them alongisde comments for the count
			var posts = _context.Posts
				.Include(p => p.User)
				.Include(p => p.Likes)
                .Include(p => p.Comments)
				.Where(p => p.User.IsPrivate == false || p.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin") || 
					_context.Follows.Any(f => f.FollowedId == p.UserId && f.FollowerId == _userManager.GetUserId(User) && f.IsAccepted == true))
                .ToList();


            ViewBag.CurrentUserId = _userManager.GetUserId(User);

            return View(posts);
        }

		[HttpGet]
        [Authorize(Roles = "User,Admin")]
		public async Task<IActionResult> HomePage()
		{
            // Get all posts from the people you follow in the order of creation
            var user = await _userManager.GetUserAsync(User);
			Console.WriteLine("User id: " + user.Id);
            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => _context.Follows.Any(f => f.FollowedId == p.UserId && f.FollowerId == user.Id && f.IsAccepted == true))
                .OrderByDescending(p => p.Date)
                .ToList();

            // Go to Index with the posts
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
				.Include(p => p.Likes)
                .FirstOrDefault(p => p.Id == id);

			// if the account is private and the logged in user is not following the person that posted the post, you cannot see the post
			if (post == null)
            {
                TempData["message"] = "Postarea nu exista!";
                TempData["messageType"] = "error";
                return RedirectToAction("Index", "Posts");
            }
			
			var user = _context.Users.FirstOrDefault(u => u.Id == post.UserId);

			if (user != null && !User.IsInRole("Admin") &&
				user.Id != _userManager.GetUserId(User) &&
				user.IsPrivate == true && 
				!_context.Follows.Any(f => f.FollowedId == user.Id &&
				f.FollowerId == _userManager.GetUserId(User) && 
				f.IsAccepted == true))
            {
                TempData["message"] = "Nu aveti dreptul sa vedeti aceasta postare!";
                TempData["messageType"] = "error";
                return RedirectToAction("Index", "Posts");
            }

			// Check if the post is liked by the current user
			TempData["IsLiked"] = false;
            if (_userManager.GetUserId(User) != null)
            {
				TempData["IsLiked"] = _context.Likes.Any(l => l.UserId == _userManager.GetUserId(User) && l.PostId == id);
            }

            ViewBag.CurrentUserId = _userManager.GetUserId(User);
			return View(post);
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Post post = new Post();
			return View(post);
		}

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> NewAsync(Post post, IFormFile? Image, string? ExistingImagePath)
        {
			var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("iframe");
			post.Date = DateTime.Now;

			if (Image != null && Image.Length > 0)
            {
                // Verificam extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Fisierul trebuie sa fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                    TempData["message"] = "Imaginea nu a putut fi adaugata!";
					TempData["messageType"] = "error";
					return View(post);
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images",Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;
                // Salvare fisier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.Image));
                post.Image = databaseFileName;
            }
			else if(ExistingImagePath != null)
			{
				post.Image = ExistingImagePath;
			}

            post.UserId = _userManager.GetUserId(User);
            if (post.Description != null)
			{
				post.Description = sanitizer.Sanitize(post.Description);
			}

			if (!ModelState.IsValid)
			{

				TempData["message"] = "Postarea nu a putut fi adaugata!";
				TempData["messageType"] = "error";

				return View(post);
			}


			_context.Posts.Add(post);
            _context.SaveChanges();

            TempData["message"] = "Postarea a fost adaugata!";
			TempData["messageType"] = "success";

			return RedirectToAction("Index", "Posts");
        }


		[HttpGet]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Edit(int id)
		{
			// check if the post exists and if the user is the owner of the post
			var post = _context.Posts.FirstOrDefault(p => p.Id == id);
			if (post == null)
			{
				TempData["message"] = "Postarea nu exista!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Home");
			}
			if (post.UserId != _userManager.GetUserId(User))
			{
				TempData["message"] = "Nu ai dreptul sa editezi aceasta postare!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}

			return View(post);
		}

		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult EditAsync(Post post, IFormFile? Image, string? ExistingImagePath)
		{
			// check if the post exists and if the user is the owner of the post
			var postToUpdate = _context.Posts.FirstOrDefault(p => p.Id == post.Id);
			if (postToUpdate == null)
			{
				TempData["message"] = "Postarea nu exista!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}
			if (postToUpdate.UserId != _userManager.GetUserId(User))
			{
				TempData["message"] = "Nu ai dreptul sa editezi aceasta postare!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}

			var sanitizer = new HtmlSanitizer();
			sanitizer.AllowedTags.Add("iframe");
			if (post.Description != null)
			{
				post.Description = sanitizer.Sanitize(post.Description);
			}

			// check if the image was changed
			if (Image != null && Image.Length > 0)
			{
                // Verificam extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
				var fileExtension = Path.GetExtension(Image.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("ArticleImage", "Fisierul trebuie sa fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
					TempData["message"] = "Imaginea nu a putut fi adaugata!";
					TempData["messageType"] = "error";
					return View(post);
				}
				// Cale stocare
				var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
				var databaseFileName = "/images/" + Image.FileName;
				// Salvare fisier
				using (var fileStream = new FileStream(storagePath, FileMode.Create))
				{
					Image.CopyTo(fileStream);
				}
				ModelState.Remove(nameof(post.Image));
				post.Image = databaseFileName;
			}
			else if(ExistingImagePath != null)
			{
                post.Image = ExistingImagePath;
			}

            postToUpdate.Description = post.Description;
            postToUpdate.Image = post.Image;
            _context.SaveChanges();
			TempData["message"] = "Postarea a fost editata!";
			TempData["messageType"] = "success";
			return RedirectToAction("Index", "Posts");
		}

		[HttpGet]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Delete(int id)
		{
			// check if the post exists and if the user is the owner of the post or an admin
			var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
			{
                TempData["message"] = "Postarea nu exista!";
                TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}
			if (post.UserId != _userManager.GetUserId(User) && !_userManager.IsInRoleAsync(_userManager.GetUserAsync(User).Result, "Admin").Result)
			{
				TempData["message"] = "Nu ai dreptul sa stergi aceasta postare!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}

			// remove the comments of the post
			if (post.Comments != null)
			{
				foreach (var comment in post.Comments)
				{
					_context.Comments.Remove(comment);
				}
			}

            _context.Posts.Remove(post);
			_context.SaveChanges();
			TempData["message"] = "Postarea a fost stearsa!";
			TempData["messageType"] = "success";
			return RedirectToAction("Index", "Posts");
		}


		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult AddComment([FromForm] Comment comment)
		{
			comment.Date = DateTime.Now;
			comment.UserId = _userManager.GetUserId(User);
			if (ModelState.IsValid)
			{
				_context.Comments.Add(comment);
				_context.SaveChanges();
				TempData["message"] = "Comentariul a fost adaugat!";
				TempData["messageType"] = "success";
				return Redirect("/Posts/Show/" + comment.PostId);
			}
			else
			{
				Post post = _context.Posts
					.Include(p => p.Comments)
					.ThenInclude(c => c.User)
					.Include(p => p.User)
					.FirstOrDefault(p => p.Id == comment.PostId);
				TempData["ShowNewComment"] = true;
				TempData["message"] = "Comentariul nu a putut fi adaugat!";
				TempData["messageType"] = "error";


				foreach (var modelState in ViewData.ModelState.Values)
				{
					foreach (var error in modelState.Errors)
					{
						TempData["NewCommentError"] = error.ErrorMessage;
					}
				}

				return Redirect("/Posts/Show/" + comment.PostId);
			}
		}


		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult EditComment([FromForm] Comment comment)
		{
			comment.Date = DateTime.Now;
			if (ModelState.IsValid)
			{

				Console.WriteLine("Comment's userID: " + comment.UserId);
				Console.WriteLine("Current user's ID: " + _userManager.GetUserId(User));
				if (comment.UserId != _userManager.GetUserId(User))
				{
					TempData["message"] = "Nu aveti dreptul sa editati acest comentariu!";
					TempData["messageType"] = "error";
					return Redirect("/Posts/Show/" + comment.PostId);
				}

				_context.Comments.Update(comment);
				_context.SaveChanges();
				TempData["message"] = "Comentariul a fost modificat!";
				TempData["messageType"] = "success";
				return Redirect("/Posts/Show/" + comment.PostId);
			}
			else
			{
				Post post = _context.Posts
					.Include(p => p.Comments)
					.ThenInclude(c => c.User)
					.Include(p => p.User)
					.FirstOrDefault(p => p.Id == comment.PostId);
				TempData["ShowEditComment"] = comment.Id;
				TempData["LastCommentContent"] = comment.Content;
				TempData["message"] = "Comentariul nu a putut fi modificat!";
				TempData["messageType"] = "error";

				foreach (var modelState in ViewData.ModelState.Values)
				{
					foreach (var error in modelState.Errors)
					{
						TempData["EditCommentError" + comment.Id] = error.ErrorMessage;
					}
				}

				return Redirect("/Posts/Show/" + comment.PostId);
			}
		}

		[HttpGet]
		[Authorize(Roles = "User,Admin")]
		public IActionResult DeleteComment(int id)
		{
			var comment = _context.Comments.FirstOrDefault(c => c.Id == id);
			if (comment == null)
			{
				TempData["message"] = "Comentariul nu exista!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}
			if (comment.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
			{
				TempData["message"] = "Nu aveti dreptul sa stergeti acest comentariu!";
				TempData["messageType"] = "error";
				return RedirectToAction("Index", "Posts");
			}
			_context.Comments.Remove(comment);
			_context.SaveChanges();
			TempData["message"] = "Comentariul a fost sters!";
			TempData["messageType"] = "success";
			return Redirect("/Posts/Show/" + comment.PostId);
		}

	}
}
