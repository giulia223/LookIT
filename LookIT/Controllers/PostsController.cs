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


        //au acces la aceasta metoda atat utilizatorii inregistrati, cat si neinregistrati si administratorii
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

        //Se afiseaza o singura postare in functie de ID-ul sau impreuna cu userul care a postat-o
        //[HttpGet] implicit

        //au acces la aceasta metoda atat utilizatorii inregistrati, cat si neinregistrati si administratorii
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

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(post);
        }

        //Se afiseaza formularul in care se vor completa datele unei posatri
        //Doar utilizatorii autentificati (users) si administrtorii pot adauga articole in platforma
        //[HttpsGet] implicit

        [Authorize(Roles = "User,Administrator")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        //se adauga postarea in baza de date 
        //Doar utilizatorii autentificati pot face postari in pltaforma sau administratorii
        [HttpPost]
        public async Task<IActionResult> New(Post post, IFormFile? Image, IFormFile? Video)
        {
            //variabile boolenane pentru a verifica daca avem continut de tip text, imagine sau videoclip
            bool hasText = !string.IsNullOrWhiteSpace(post.TextContent);
            bool hasImage = Image != null && Image.Length > 0;
            bool hasVideo = Video != null && Video.Length > 0;
            post.Date = DateTime.Now;

            //preluam ID-ul utilizatorului care posteaza
            post.AuthorId = _userManager.GetUserId(User);

            if (!hasText && !hasImage && !hasVideo)
            {
                ModelState.AddModelError(string.Empty, "Postarea nu poate fi goală!");
                return View(post);
            }
            if (hasImage)
            {
                //Verificam extensia pentru imagine
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Fisierul trebuie sa fie imagine (jpg, jpeg, png, gif).");
                    return View(post);
                }

                //Cale stocare
                //generam un GUID pentru a stoca fisiere sub o denumire unica 
                //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,poza.jpg"
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "images", "posts");
                var databaseFileName = "/images/posts/" + uniqueFileName;

                //daca nu exista directorul in wwwroot, atunci il vom crea
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

            if (hasVideo)
            {
                //Verificam extensia pentru videoclip
                var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".avi", ".mkv" };

                var fileExtension = Path.GetExtension(Video.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Video", "Fisierul trebuie sa fie videclip (mp4, mov, webm, avi, mkv etc).");
                    return View(post);
                }

                //Cale stocare
                //generam un GUID pentru a stoca fisiere sub o denumire unica 
                //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,video.mp4"
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "videos", "posts");
                var databaseFileName = "/videos/posts/" + uniqueFileName;

                //daca nu exista directorul in wwwroot, atunci il vom crea
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }

                var filePath = Path.Combine(storagePath, uniqueFileName);

                //Salvare fisier
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Video.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.VideoUrl));

                post.VideoUrl = databaseFileName;
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
        public async Task<IActionResult> Edit(int Id, Post requestPost, IFormFile? Image, IFormFile? Video, bool DeleteImage, bool DeleteVideo)
        {
            Post? post = await db.Posts.FindAsync(Id);

            if (post == null)
            {
                return NotFound();
            }

            if ((post.AuthorId == _userManager.GetUserId(User)) || User.IsInRole("Administrator"))
            {
                if (ModelState.IsValid)
                {
                    
                    string? initialTextContent = post.TextContent;
                    string? initialImageUrl = post.ImageUrl;
                    string? initialVideoUrl = post.VideoUrl;

                    post.TextContent = requestPost.TextContent;

                    if (Image != null && Image.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Image", "Fișierul trebuie să fie imagine.");
                            return View(post);
                        }

                        // Ștergem vechea imagine fizic DOAR dacă punem una nouă în loc (asta e ok)
                        if (!string.IsNullOrEmpty(post.ImageUrl))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var storagePath = Path.Combine(_env.WebRootPath, "images", "posts");
                        if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                        var filePath = Path.Combine(storagePath, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }
                        post.ImageUrl = "/images/posts/" + uniqueFileName;
                    }

                    else if (DeleteImage == true)
                    {
                        post.ImageUrl = null;
                    }

                    if (Video != null && Video.Length > 0)
                    {
                        var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".avi", ".mkv" };
                        var fileExtension = Path.GetExtension(Video.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Video", "Format video neacceptat.");
                            return View(post);
                        }

                        if (!string.IsNullOrEmpty(post.VideoUrl))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, post.VideoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var storagePath = Path.Combine(_env.WebRootPath, "videos", "posts");
                        if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                        var filePath = Path.Combine(storagePath, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Video.CopyToAsync(fileStream);
                        }
                        post.VideoUrl = "/videos/posts/" + uniqueFileName;
                    }
                    else if (DeleteVideo == true)
                    {
                        post.VideoUrl = null;
                    }

                    if (string.IsNullOrWhiteSpace(post.TextContent) && post.ImageUrl == null && post.VideoUrl == null)
                    {
                        post.TextContent = initialTextContent;
                        post.ImageUrl = initialImageUrl;
                        post.VideoUrl = initialVideoUrl;

                        ModelState.Clear();

                        ModelState.AddModelError(string.Empty, "Nu poți șterge tot conținutul! Modificările au fost anulate.");
                        return View(post);
                    }
                    // Dacă utilizatorul a vrut să șteargă imaginea (DeleteImage=true) și ea a fost setată pe null în obiect,
                    // dar exista o imagine inițială -> Acum o ștergem de pe disc.
                    if (post.ImageUrl == null && !string.IsNullOrEmpty(initialImageUrl) && DeleteImage == true)
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, initialImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // La fel pentru video
                    if (post.VideoUrl == null && !string.IsNullOrEmpty(initialVideoUrl) && DeleteVideo == true)
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, initialVideoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    await db.SaveChangesAsync();

                    TempData["message"] = "Postarea a fost modificată cu succes!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(post);
                }
            }
            else
            {
                TempData["message"] = "Nu aveți dreptul să modificați această postare.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
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

        //Conditiile de afisare pentru butoanele de editare si steregere
        //butoanele sunt aflate in view-uri
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdministrator = User.IsInRole("Administrator");
        }
        
    } 
}
