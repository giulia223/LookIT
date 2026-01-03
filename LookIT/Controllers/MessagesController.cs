using Ganss.Xss;
using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace LookIT.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
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
        public async Task<IActionResult> New(Message msg, IFormFile? Image, IFormFile? Video)
        {
            //variabile boolenane pentru a verifica daca avem continut de tip text, imagine sau videoclip
            bool hasText = !string.IsNullOrWhiteSpace(msg.TextContent);
            bool hasImage = Image != null && Image.Length > 0;
            bool hasVideo = Video != null && Video.Length > 0;
            // preluam Id-ul utilizatorului care face mesajul
            msg.UserId = _userManager.GetUserId(User);

            msg.Date = DateTime.Now;

            if (!hasText && !hasImage && !hasVideo)
            {
                ModelState.AddModelError(string.Empty, "Mesajul nu poate fi gol!");
                return View(msg);
            }
            if (hasImage)
            {
                //verificam extensia pentru imagine
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Fisierul trebuie sa fie imagine (jpg, jpeg, png, gif).");
                    return View(msg);
                }

                //generam un GUID pentru a stoca fisiere sub o denumire unica 
                //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,poza.jpg"
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "images", "messages");
                var databaseFileName = "/images/messages/" + uniqueFileName;

                //daca nu exista directorul in wwwroot, atunci il vom crea
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }

                var filePath = Path.Combine(storagePath, uniqueFileName);

                //salvare fisier
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(msg.ImageUrl));

                msg.ImageUrl = databaseFileName;
            }

            if (hasVideo)
            {
                //verificam extensia pentru videoclip
                var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".avi", ".mkv" };

                var fileExtension = Path.GetExtension(Video.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Video", "Fisierul trebuie sa fie videoclip (mp4, mov, webm, avi, mkv etc).");
                    return View(msg);
                }

                //Cale stocare
                //generam un GUID pentru a stoca fisiere sub o denumire unica 
                //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,video.mp4"
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "videos", "messages");
                var databaseFileName = "/videos/messages/" + uniqueFileName;

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
                ModelState.Remove(nameof(msg.VideoUrl));

                msg.VideoUrl = databaseFileName;
            }

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
            SetAccessRights();
            var msg = _context.Messages.Find(Id);
            var userId = _userManager.GetUserId(User);
            if (msg == null)
            {
                return NotFound();
            }
            if (msg.UserId != userId && !(User.IsInRole("Administrator")))
            {
                TempData["message"] = "Nu aveti dreptul sa editati un mesaj care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new { Id = msg.GroupId });
            }
            ViewBag.MessageDetails = msg;
            return View(msg);
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public async Task<IActionResult> Edit(int Id, Message requestmsg, IFormFile? Image, IFormFile? Video, bool DeleteImage, bool DeleteVideo)
        {
            SetAccessRights();
            var sanitizer = new HtmlSanitizer();
            Message? msg = _context.Messages.Find(Id);
            var grpId = msg.GroupId;

            if (msg is null)
            {
                return NotFound();
            }
            else
            {
                if (msg.UserId == _userManager.GetUserId(User) || (User.IsInRole("Administrator")))
                {

                    if (ModelState.IsValid)
                    {
                        //campurile initiale ale postarii
                        string? initialTextContent = msg.TextContent;
                        string? initialImageUrl = msg.ImageUrl;
                        string? initialVideoUrl = msg.VideoUrl;

                        msg.TextContent = requestmsg.TextContent;

                        //daca am ales o poza, o vom modifica
                        if (Image != null && Image.Length > 0)
                        {
                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                            var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("Image", "Fișierul trebuie să fie imagine.");
                                return View(msg);
                            }

                            // stergea vechea imagine fizic din wwwroot/images/posts DACA EXISTA 
                            if (!string.IsNullOrEmpty(msg.ImageUrl))
                            {
                                var oldPath = Path.Combine(_env.WebRootPath, msg.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }

                            //generam un GUID pentru a stoca fisiere sub o denumire unica 
                            //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,poza.jpg"
                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var storagePath = Path.Combine(_env.WebRootPath, "images", "messages");

                            //daca nu exista directorul images in wwwroot, atunci il vom crea
                            if (!Directory.Exists(storagePath))
                            {
                                Directory.CreateDirectory(storagePath);
                            }

                            var filePath = Path.Combine(storagePath, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await Image.CopyToAsync(fileStream);
                            }
                            msg.ImageUrl = "/images/messages/" + uniqueFileName;
                        }

                        //daca am selectat totusi sa stergem imginea curenta, doar vom seta url ul pe null la acest pas
                        //stergerea fizica a imaginii din wwwroot se va face dupa verificarea finala: sa nu avem o postare goala
                        //in ruma editarii; in acest caz, vom pastra toate campurile initiale
                        else if (DeleteImage == true)
                        {
                            msg.ImageUrl = null;
                        }

                        //daca am ales un video, o vom modifica
                        if (Video != null && Video.Length > 0)
                        {
                            var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".avi", ".mkv" };
                            var fileExtension = Path.GetExtension(Video.FileName).ToLower();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("Video", "Format video neacceptat.");
                                return View(msg);
                            }

                            // stergea vechiul videoclip din wwwroot/videos/posts DACA EXISTA
                            if (!string.IsNullOrEmpty(msg.VideoUrl))
                            {
                                var oldPath = Path.Combine(_env.WebRootPath, msg.VideoUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }

                            //generam un GUID pentru a stoca fisiere sub o denumire unica 
                            //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,video.mp4"
                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var storagePath = Path.Combine(_env.WebRootPath, "videos", "posts");

                            //daca nu exista directorul videos in wwwroot, atunci il vom crea
                            if (!Directory.Exists(storagePath))
                            {
                                Directory.CreateDirectory(storagePath);
                            }

                            var filePath = Path.Combine(storagePath, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await Video.CopyToAsync(fileStream);
                            }
                            msg.VideoUrl = "/videos/messages/" + uniqueFileName;
                        }

                        //daca am selectat totusi sa stergem videoclipul curent, doar vom seta url ul pe null la acest pas
                        //stergerea fizica a videoclipului din wwwroot se va face dupa verificarea finala: sa nu avem o postare goala
                        //in ruma editarii; in acest caz, vom pastra toate campurile initiale
                        else if (DeleteVideo == true)
                        {
                            msg.VideoUrl = null;
                        }

                        //daca am editat si am ajuns la o postare goala (toate cele 3 campuri TextContent, ImageUrl si VideoUrl sunt
                        //nule), nu pot face modiifcarea si voi mentine starea intiiala a postarii
                        //ne intoarcem in view cu valorile initiale
                        if (string.IsNullOrWhiteSpace(msg.TextContent) && msg.ImageUrl == null && msg.VideoUrl == null)
                        {
                            msg.TextContent = initialTextContent;
                            msg.ImageUrl = initialImageUrl;
                            msg.VideoUrl = initialVideoUrl;

                            ModelState.Clear();

                            ModelState.AddModelError(string.Empty, "Nu poți șterge tot conținutul! Modificările au fost anulate.");
                            return View(msg);
                        }
                        //mai jos ajungem in cazul in care cel putin unul din campurile postarii este nenul

                        // daca utilizatorul a vrut sa stearga imaginea (DeleteImage=true) și ea a fost setata pe null in obiect,
                        // dar exista o imagine initiala, atunci acum o stergem
                        if (msg.ImageUrl == null && !string.IsNullOrEmpty(initialImageUrl) && DeleteImage == true)
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, initialImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // daca utilizatorul a vrut sa stearga videoclipul (DeletevVideo=true) și a fost setat pe null in obiect,
                        // dar exista un videoclip initial, atunci acum il stergem
                        if (msg.VideoUrl == null && !string.IsNullOrEmpty(initialVideoUrl) && DeleteVideo == true)
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, initialVideoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }
                        msg.TextContent = sanitizer.Sanitize(requestmsg.TextContent);
                        TempData["message"] = "Mesajul a fost modificat";
                        TempData["messageType"] = "alert-success";
                        _context.SaveChanges();
                        return RedirectToAction("Show", "Groups", new { Id = msg.GroupId });
                    }
                    else
                    {
                        //avem nevoie sa reatribuim la requestPost imaginea si videoclipul postarii pentru a nu o pierde
                        requestmsg.ImageUrl = msg.ImageUrl;
                        requestmsg.VideoUrl = msg.VideoUrl;

                        return View(requestmsg);
                    }
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui mesaj care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", "Groups", new {Id = grpId});
                }

            }

        }

        //stergere mesaj 
        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public async Task<IActionResult> DeleteMessage(int Id)
        {
            SetAccessRights();
            var msg = _context.Messages.Find(Id);
            var groupId = msg.GroupId;
            var userId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);
            if (msg == null)
            {
                return NotFound();
            }
            if (msg.UserId != userId && !(await _userManager.IsInRoleAsync(currentUser, "Administrator")))
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un mesaj care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new {Id = groupId});
            }
            //stergem fizic din wwwroot/images/posts imaginea, daca exista
            if (!string.IsNullOrEmpty(msg.ImageUrl))
            {
                var imagePath = Path.Combine(_env.WebRootPath, msg.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            //stergem fizic din wwwroot/videos/posts videocliupl, daca exista
            if (!string.IsNullOrEmpty(msg.VideoUrl))
            {
                var videoPath = Path.Combine(_env.WebRootPath, msg.VideoUrl.TrimStart('/'));
                if (System.IO.File.Exists(videoPath))
                {
                    System.IO.File.Delete(videoPath);
                }
            }

            _context.Messages.Remove(msg);
            _context.SaveChanges();
            TempData["message"] = "Mesaj sters.";
            TempData["messageType"] = "alert-danger";
           
            return RedirectToAction("Show", "Groups", new {Id = groupId});
        }

        private void SetAccessRights()
        {
            ViewBag.EsteUser = false;

            if (User.IsInRole("User"))
            {
                ViewBag.EsteUser = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdministrator = User.IsInRole("Administrator");
        }

    }
}
