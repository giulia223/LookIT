using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class LikesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _env = env;

        [Authorize(Roles ="User,Administrator")]
        public IActionResult Index()
        {
            var likedPosts = db.Likes
                               .Where(like => like.UserId == _userManager.GetUserId(User))
                               .Include(like => like.Post)
                               .ThenInclude(post => post.Author)
                               .OrderByDescending(like => like.LikeId)
                               .Select(like => like.Post) 
                               .ToList();

            ViewBag.Message = TempData["message"];
            ViewBag.Alert = TempData["messageType"];

            return View(likedPosts);
        }
    }

}
