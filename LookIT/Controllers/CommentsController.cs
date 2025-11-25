using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class CommentsController : Controller
    {

        private readonly ApplicationDbContext _context;
        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        ////afisare toate comentariile
        //public IActionResult Index()
        //{
        //    var comments = _context.Comments.Include(c => c.User).ToList();

        //    ViewBag.Comments = comments;
        //    return View();
        //}

        ////stergere comentariu fara user specific

        //[HttpPost]
        //public IActionResult DeleteCommentsNoUser(int Id)
        //{
        //    var comm = _context.Comments.Find(Id);
        //    if (comm == null)
        //    {
        //        return NotFound();
        //    }
        //    _context.Comments.Remove(comm);
        //    _context.SaveChanges();

        //    return RedirectToAction("AllComments");
        //}
    }
}
