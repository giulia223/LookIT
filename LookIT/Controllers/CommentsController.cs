using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LookIT.Services;

namespace LookIT.Controllers
{
    public class CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IModerationService moderationService) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IModerationService _moderationService = moderationService;

        //stergerea unui comentariu asociat unei postari din baza de date
        //se poate sterge coemntariul doar de catre useruii cu rolul de Administrator 
        //sau de catre utilizatorii cu rolul de User, doar daca comentariul a fost postat de catre acestia

        [HttpPost]
        [Authorize(Roles ="User,Administrator")]
        public IActionResult Delete(int Id)
        {
            Comment? comment = db.Comments.Find(Id);

            if (comment is null)
            {
                return NotFound();
            }

            else
            {
                if(comment.UserId == _userManager.GetUserId(User)
                 || User.IsInRole("Administrator"))
                {
                    db.Comments.Remove(comment);
                    db.SaveChanges();
                    return Redirect("/Posts/Show/" + comment.PostId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti comentariul.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts");
                }
            }

        }

        //se editeaza un comentariu existent asociat unei postari
        //se poate edita un comentariu doar de catre utilizatorul care a postat comentariul respectiv  sau de catre administratori,
        //chiar daca nu acestia sunt autorii comentariului

        [Authorize(Roles ="User,Administrator")]
        public IActionResult Edit(int Id)
        {
            Comment? comment = db.Comments.Find(Id);

            if(comment is null)
            {
                return NotFound();
            }
            else
            {
                if(comment.UserId == _userManager.GetUserId(User) 
                    || User.IsInRole("Administrator"))
                {
                    return View(comment);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa editati comentariul.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts");
                }
            }
        }

        [HttpPost]
        [Authorize(Roles="User,Administrator")]
        public async Task<IActionResult> Edit(int Id, Comment requestComment)
        {
            Comment? comment = db.Comments.Find(Id);

            if(comment is null)
            {
                return NotFound();
            }
            else
            {
                if (comment.UserId == _userManager.GetUserId(User)
                    || User.IsInRole("Administrator"))
                {
                    if (ModelState.IsValid)
                    {

                        var moderationResult = await _moderationService.CheckContentAsync(requestComment.Content);

                        if (moderationResult.Success)
                        {
                            if (moderationResult.IsFlagged is true)
                            {
                                ModelState.AddModelError("Content", $"Comentariul nu fost editat deoarece incalca regulile comunitatii: {moderationResult.Reason}");

                                //ne asiguram ca comentariul pastreaza id-ul corect pentru a se intoarce in view ul de editare
                                requestComment.CommentId = Id;
                                return View(requestComment);
                            }

                            comment.IsFlagged = moderationResult.IsFlagged;
                            comment.FlagCategory = "Safe";
                        }

                        comment.Content = requestComment.Content;
                        comment.DateModified = DateTime.Now;

                        db.SaveChanges();

                        return RedirectToAction("Show", "Posts", new { id = comment.PostId});
                    }
                    else
                    {
                        return View(requestComment);
                    }
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa editati comentariul.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts");
                }
            }
        } 
    }
}
