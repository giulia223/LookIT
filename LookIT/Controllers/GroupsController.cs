using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{

    public class GroupsController : Controller
    {

        private readonly ApplicationDbContext _context;
        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        ////afisare toate grupurile
        //public IActionResult Index()
        //{
        //    var groups = _context.Groups.Include(g => g.Messages).ToList();

        //    ViewBag.Groups = groups;
        //    return View();
        //}

        ////stergere mesaj din grup
        //[HttpPost]
        //public IActionResult DeleteMessageFromGroup(int Id)
        //{
        //    var msg = _context.Messages.Find(Id);
        //    if (msg == null)
        //    {
        //        return NotFound();
        //    }
        //    _context.Messages.Remove(msg);
        //    _context.SaveChanges();

        //    return RedirectToAction("AllGroups");
        //}

        ////stergere grup 
        //[HttpPost]
        //public IActionResult DeleteGroupNoUser(int Id)
        //{
        //    var grp = _context.Groups.Find(Id);
        //    if (grp == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Groups.Remove(grp);
        //    _context.SaveChanges();

        //    return RedirectToAction("AllGroups");

        //}
    }
}
