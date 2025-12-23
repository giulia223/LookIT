using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace LookIT.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GroupMembersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //afisare grupurile unui user
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Show()
        {
            var gp = _context.GroupMembers.Where(gm => gm.MemberId == _userManager.GetUserId(User))
                                                    .Where(gm => gm.Status == "moderator" || gm.Status == "Accepted")
                                                    .Include(gm => gm.Group)
                                                    .ThenInclude(gp => gp.Moderator)
                                                    .ToList();

            ViewBag.GroupsUser = gp;
            ViewBag.GroupsCount = gp.Count;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        //stergere membru din grup
        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult RemoveMember(int GroupId, string MemberId)
        {
            var groupMember = _context.GroupMembers
                .FirstOrDefault(gm => gm.GroupId == GroupId && gm.MemberId == MemberId);

            if (groupMember == null)
            {
                return NotFound();
            }

            var groupModerator = _context.GroupMembers
            .FirstOrDefault(gm => gm.GroupId == GroupId && gm.MemberId == _userManager.GetUserId(User));

            if (groupModerator.Status != "moderator")
            {
                TempData["message"] = "Nu ai permisiunea de a elimina membri din acest grup.";
                TempData["messageType"] = "danger";
                return RedirectToAction("Show", "Groups", new { Id = GroupId });
            }
            _context.GroupMembers.Remove(groupMember);
            _context.SaveChanges();
            TempData["message"] = "Membrul a fost eliminat din grup.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", "Groups", new { Id = GroupId });
        }

        //acceptare cerere de membru
        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult AcceptMember(int groupId, string memberId)
        {
            var groupMember = _context.GroupMembers
                .FirstOrDefault(gm => gm.GroupId == groupId && gm.MemberId == memberId);
            
            if (groupMember == null)
            {
                return NotFound();
            }

            var groupModerator = _context.GroupMembers
                .FirstOrDefault(gm => gm.GroupId == groupId && gm.MemberId == _userManager.GetUserId(User));

            if (groupModerator.Status == "moderator")
            {
                groupMember.Status = "Accepted";
                _context.SaveChanges();
                TempData["message"] = "Cererea de membru a fost acceptată.";
                TempData["messageType"] = "success";
                return RedirectToAction("Show", "Groups", new { Id = groupId });
            }
            else
            {
                TempData["message"] = "Nu ai permisiunea de a accepta membri în acest grup.";
                TempData["messageType"] = "danger";
                return RedirectToAction("Show", "Groups", new { Id = groupId });
            }
           

        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult LeaveGroup(int groupId)
        {

            var groupMember = _context.GroupMembers
                .FirstOrDefault(gm => gm.GroupId == groupId && gm.MemberId == _userManager.GetUserId(User));
            if (groupMember == null)
            {
                return NotFound();
            }
            _context.GroupMembers.Remove(groupMember);
            _context.SaveChanges();
            TempData["message"] = "Ai părăsit grupul.";
            TempData["messageType"] = "success";
            return RedirectToAction("Show", "GroupMembers");
        }

    }
}
