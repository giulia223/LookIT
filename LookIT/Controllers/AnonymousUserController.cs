using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class AnonymousUserController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;
        public AnonymousUserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //// Utilizatorii nelogați POT vedea postările
        //public IActionResult ViewPosts()
        //{
        //    var posts = _context.Posts.
        //        Include(p => p.Likes).
        //        Include(p => p.Comments).ToList();
        //    ViewBag.Postari = posts;
        //    return View(); 
        //}

        //// Utilizatorii nelogați POT vedea detaliile unui grup
        //public IActionResult ViewGroupDetails()
        //{
        //    var groups = _context.Groups.ToList();
        //    ViewBag.grupuri = groups;
        //    return View(); 
        //}

        //// Utilizatorii nelogați POT vedea comentariile
        //public IActionResult ViewComments(int postId)
        //{
        //    var comm = _context.Comments.Find(postId);
        //    return View(comm); 
        //}

    }
}
