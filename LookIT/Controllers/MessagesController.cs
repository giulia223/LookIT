using LookIT.Data;
using LookIT.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        //afisare toate mesajele
        [Authorize(Roles = "Administrator")]
        public IActionResult Index()
        {
            var messages = _context.Messages.Include(m => m.User).ToList();

            ViewBag.Messages = messages;
            return View();
        }

        //afisare mesajele unui grup
        [Authorize(Roles = "User,Administrator")]
        public IActionResult ShowMessages(int groupId)
        {
            var messages = _context.Messages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .ToList();
            ViewBag.Messages = messages;
            ViewBag.GroupId = groupId;
 
            return View();
        }

        //afisare mesaj dupa Id
        [Authorize(Roles = "Administrator")]
        public IActionResult ShowMessage(int Id)
        {
            var msg = _context.Messages
                .Where(m => m.MessageId == Id)
                .Include(m => m.User)
                .Include(m => m.Group)
                .FirstOrDefault();
            if (msg == null)
            {
                return NotFound();
            }
            ViewBag.MessageDetails = msg;
            return View();
        }

        //creare mesaj
        [Authorize(Roles = "User,Administrator")]
        public IActionResult New(int id)
        {
            Message msg = new Message();
            msg.GroupId = id;
            return View(msg);
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult New(Message msg)
        {
            // preluam Id-ul utilizatorului care face mesajul
            msg.UserId = _userManager.GetUserId(User);
            
            msg.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Messages.Add(msg);
                _context.SaveChanges();
               
                TempData["message"] = "Mesajul a fost trimis cu succes";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Show", "Groups", new {Id = msg.GroupId});
            }

            else
            {
                TempData["message"] = "Eroare la trimiterea mesajului. Verificati campurile.";
                TempData["messageType"] = "alert-danger";
                ViewBag.Message = TempData["message"].ToString();
                return View(msg);
            }
        }


        //editare mesaj
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Edit(int Id)
        {
            var msg = _context.Messages.Find(Id);
            var userId = _userManager.GetUserId(User);
            if (msg == null)
            {
                return NotFound();
            }
            if (msg.UserId != userId)
            {
                TempData["message"] = "Nu aveti dreptul sa editati un mesaj care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new { Id = msg.GroupId });
            }
            ViewBag.MessageDetails = msg;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Edit(int Id, Message requestmsg)
        {
            var sanitizer = new HtmlSanitizer();
            Message? msg = _context.Messages.Find(Id);

            if (msg is null)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (msg.UserId == _userManager.GetUserId(User))
                       
                    {
                        msg.Content = sanitizer.Sanitize(requestmsg.Content);
                        TempData["message"] = "Mesajul a fost modificat";
                        TempData["messageType"] = "alert-success";
                        _context.SaveChanges();
                        return RedirectToAction("Show", msg.GroupId);
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui mesaj care nu va apartine";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Show");
                    }
                }
                else
                {                   
                    return View(requestmsg);
                }
            }

        }

        //stergere mesaj 
        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public async Task<IActionResult> DeleteMessage(int Id)
        {
            var msg = _context.Messages.Find(Id);
            var groupId = msg.GroupId;
            var userId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);
            if (msg == null)
            {
                return NotFound();
            }
            if (msg.UserId != userId || await _userManager.IsInRoleAsync(currentUser, "Administrator"))
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un mesaj care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new {Id = groupId});
            }
            _context.Messages.Remove(msg);
            _context.SaveChanges();
            TempData["message"] = "Mesaj sters.";
            TempData["messageType"] = "alert-danger";
           
            return RedirectToAction("Show", "Groups", new {Id = groupId});
        }

    }
}
