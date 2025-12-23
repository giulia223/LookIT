using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class AdministratorsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdministratorsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager; 
            _context = context;
            _roleManager = roleManager;
        }
        [Authorize (Roles = "Administrator")]
        public IActionResult Page()
        {
            SetAccessRights();

            if (ViewBag.Afisare == false)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // AFISARI PENTRU USERS

        //afisare toti useri
        [Authorize(Roles = "Administrator")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            SetAccessRights();

            if (ViewBag.Afisare == false)
            {
                return RedirectToAction("Index", "Home");
            }

            var users = _userManager.Users.ToList();
            ViewBag.Users = users;

            return View();
        }

        //afisare user dupa id
        public async Task<IActionResult> Show(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // afișare postări pentru un user
        public async Task<IActionResult> ManageContent(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userWposts = await _context.ApplicationUsers
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userWposts == null)
            {
                return NotFound();
            }

            ViewBag.Posts = userWposts.Posts;
            ViewBag.User = userWposts;

            return View();
        }

        // afișare comentarii pentru un user
        public async Task<IActionResult> ManageComm(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userWcomm = await _context.ApplicationUsers
                .Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userWcomm == null)
            {
                return NotFound();
            }


            ViewBag.Comments = userWcomm.Comments;
            ViewBag.User = userWcomm;

            return View();
        }

        // afișare grupuri pentru un user
        public async Task<IActionResult> ManageGroups(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userWgroups = await _context.ApplicationUsers
                 .Include(u => u.Groups)
                 .ThenInclude(gm => gm.Group)
                 .FirstOrDefaultAsync(u => u.Id == id);

            if (userWgroups == null)
            {
                return NotFound();
            }

            ViewBag.Groups = userWgroups.Groups;
            ViewBag.User = userWgroups;

            return View();
        }

        // afișare mesaje pentru un user
        public async Task<IActionResult> ManageMessages(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userWmsgs = await _context.ApplicationUsers
                .Include(u => u.SentMessages)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userWmsgs == null)
            {
                return NotFound();
            }

            ViewBag.Messages = userWmsgs.SentMessages;
            ViewBag.User = userWmsgs;
            return View();
        }

        // ACTIUNI DE STERGERE PENTRU USERS (POSTARI, COMENTARII, GRUPURI, MESAJE)

        //stergere user

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            //utilizatorul nu a fost gasit
            if (user == null)
            {
                return NotFound();
            }

            //nu trebuie sa sterg postarile manual pentru ca am setat OnDeleteCascade la relatia dintre "un user poate avea mai multe postari"
            //totusi, trebuie sa sterg mai intai comentariile asociate unui utilizator pentru a putea sterge userul
            //caci am setat OnDeleteRestrict pentru evitarea ciclurilor

            var comments = _context.Comments
                                    .Where(comment => comment.UserId == Id)
                                    .ToList();

            //daca avem comentarii, le vom sterge manual
            if(comments.Count > 0)
            {
                _context.Comments.RemoveRange(comments);
                await _context.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["message"] = "Utilizator sters.\n";
                return RedirectToAction("Index");
            }
            else
            {
                return View("Error");
            }
        }

        //stergere postare pentru un user

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeletePost(int Id)
        {
            var post = _context.Posts.Find(Id);
            if (post == null)
            {
                return NotFound();
            }

            var author = post.AuthorId;

            _context.Posts.Remove(post);
            _context.SaveChanges();

            return RedirectToAction("ManageContent", new { id = author });
        }

       
        //stergere comentariu pentru un user

        [HttpPost]
        [Authorize(Roles = "Administrator")]

        public IActionResult DeleteComments(int Id)
        {
            var comm = _context.Comments.Find(Id);
            if (comm == null)
            {
                return NotFound();
            }

            var author = comm.UserId;

            _context.Comments.Remove(comm);
            _context.SaveChanges();

            return RedirectToAction("ManageComm", new { id = author });
        }

       

        //stergere mesaj pentru un user

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteMessage(int Id)
        {
            var msg = _context.Messages.Find(Id);
            if (msg == null)
            {
                return NotFound();
            }

            var author = msg.UserId;

            _context.Messages.Remove(msg);
            _context.SaveChanges();

            return RedirectToAction("ManageMessages", new { id = author });
        }

      
        //stergere grup pentru user

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteGroup(int Id, string userId)
        {
            var grp = _context.Groups.Find(Id);
            if (grp == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(grp);
            _context.SaveChanges();

            return RedirectToAction("ManageGroups", new { id = userId });
        }


        // ACTIUNE PENTRU GESTIONAREA ROLURILOR
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PromoteToAdms(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            string admsRoleName = "Administrator";

            // Verifică dacă utilizatorul este deja administrator
            if (await _userManager.IsInRoleAsync(user, admsRoleName))
            {
                
                return RedirectToAction("Index");
            }

            // Atribuie rolul de Administrator
            var result = await _userManager.AddToRoleAsync(user, admsRoleName);
            TempData["message"] = "Utilizatorul a devenit admin.\n";

            return RedirectToAction("Index");

            //if (result.Succeeded)
            //{
            //    //TempData["StatusMessage"] = $"Utilizatorul {user.UserName} a fost promovat cu succes.";
            //    return RedirectToAction("Index");
            //}
            //else
            //{
            //    // Gestionează erorile, de exemplu dacă rolul nu a fost găsit
            //    // TempData["ErrorMessage"] = $"Eroare la promovare: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            //    return RedirectToAction("Index");
            //}
        }

        private void SetAccessRights()
        {
            ViewBag.Afisare = false;

            if (User.IsInRole("Administrator"))
            {
                ViewBag.Afisare = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Administrator");
        }


    }
}
