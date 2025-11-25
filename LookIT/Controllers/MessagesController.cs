using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        ////afisare toate mesajele
        //public IActionResult Index()
        //{
        //    var messages = _context.Messages.Include(m => m.User).ToList();

        //    ViewBag.Messages = messages;
        //    return View();
        //}

        ////stergere mesaj 
        //[HttpPost]
        //public IActionResult DeleteMessageNoUser(int Id)
        //{
        //    var msg = _context.Messages.Find(Id);
        //    if (msg == null)
        //    {
        //        return NotFound();
        //    }
        //    _context.Messages.Remove(msg);
        //    _context.SaveChanges();

        //    return RedirectToAction("AllMessages");
        //}

    }
}
