using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        ////afisare toate postarile
        //public IActionResult Index()
        //{
        //    var posts = _context.Posts.Include(p => p.Comments).ToList();

        //    ViewBag.Posts = posts;
        //    return View();
        //}

        ////afisare postare 

        //public ActionResult Show(int id)
        //{
        //    Post postare = _context.Posts.Find(id);
        //    return View(postare);
        //}

        ////stergere postare
        //[HttpPost]
        //public IActionResult DeletePostNoUser(int Id)
        //{
        //    var post = _context.Posts.Find(Id);
        //    if (post == null)
        //    {
        //        return NotFound();
        //    }
        //    _context.Posts.Remove(post);
        //    _context.SaveChanges();

        //    return RedirectToAction("AllPosts");
        //}


        ////stergere comentariu pentru o postare

        //[HttpPost]
        //public IActionResult DeleteCommentsFromPost(int Id)
        //{
        //    var comm = _context.Comments.Find(Id);
        //    if (comm == null)
        //    {
        //        return NotFound();
        //    }
        //    _context.Comments.Remove(comm);
        //    _context.SaveChanges();

        //    return RedirectToAction("AllPosts");
        //}


    }
}
