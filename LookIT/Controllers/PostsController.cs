using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LookIT.Controllers
{
    public class PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _env = env;


        [AllowAnonymous]
        public IActionResult Index()
        {
            var posts = db.Posts
                .Include(post => post.Author)
                .OrderByDescending(post => post.Date);

            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }

        //Se afiseaza o singura postare in functie de ID-ul sau
        //impreuna cu userul care l-a postat
        //[HttpGet] implicit

        [AllowAnonymous]
        public IActionResult Show(int id)
        {
            Post? post = db.Posts
                .Include(post => post.Author)
                .Where(post => post.PostId == id)
                .FirstOrDefault();

            if (post is null)
            {
                return NotFound();
            }
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(post);
        }

        //Se afiseaza formularul in care se vor completa datele unei posatri
        //Doar utilizatorii autentificati (users) pot adauga articole in platforma
        //[HttpsGet] implicit

        [Authorize(Roles = "User,Administrator")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        //se adauga postarea in baza de date 
        //Doar utilizatorii autentificati pot face postari in pltaforma
        [HttpPost]
        public async Task<IActionResult> New(Post post, IFormFile? Image)
        {
            post.Date = DateTime.Now;

            //preluam id-ul utilizatorului care posteaza
            post.AuthorId = _userManager.GetUserId(User);

            if (Image != null && Image.Length > 0)
            {
                //Verificam extensia 
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Fisierul trebuie sa fie imagine (jpg, jpeg, png, gif) sau video (mp4, mov).");
                    return View(post);
                }

                //Cale stocare
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "images", "posts");
                var databaseFileName = "/images/posts/" + uniqueFileName;

                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }

                var filePath = Path.Combine(storagePath, uniqueFileName);

                //Salvare fisier
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.ImageUrl));

                post.ImageUrl = databaseFileName;
            }

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost adaugata cu succes!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                return View(post);
            }
        }

        //Se editeaza o postare in baza de date
        //Se afiseaza formularul impreuna cu datele aferente postarii
        //Doar userii al caror postari le apartin pot edita postarile
        //[HttpGet] implicit

        [Authorize(Roles = "Administrator,User")]
        public IActionResult Edit(int Id)
        {
            Post? post = db.Posts
                .Where(post => post.PostId == Id)
                .FirstOrDefault();

            if (post == null)
            {
                return NotFound();
            }

            if (post.AuthorId == _userManager.GetUserId(User)
                || User.IsInRole("Administrator"))
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        //Se adauga postarea in baza de date 
        //Se verifica rolul utilizatorului care are dreptul sa editeze
        
        [HttpPost]
        public IActionResult Edit(int Id, Post requestPost)
        {
            Post? post = db.Posts.Find(Id);

            if(post is null)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if((post.AuthorId == _userManager.GetUserId(User))
                        || User.IsInRole("Administrator"))
                    {
                        //mai am de pus aici
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(requestPost);
                }
            }
        }

        //Se sterge o postare din baza de date 
        //Doar utilizatorii autentificati care au facut postarea respetiva
        //o pot sterge

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Delete(int Id)
        {
            Post? post = db.Posts
                .Where(post => post.PostId== Id)
                .FirstOrDefault();

            if(post is null)
            {
                return NotFound();
            }
            else
            {
                if((post.AuthorId == _userManager.GetUserId(User))
                    || User.IsInRole("Administrator"))
                {
                    db.Posts.Remove(post);
                    db.SaveChanges();
                    TempData["message"] = "Postarea a fost stearsa cu succes.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti o postare care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
        }
        
    } 
}
