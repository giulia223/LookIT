using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
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
            
            var userId = _userManager.GetUserId(User);
            List<string> followingUserIds = new List<string>();


            //daca utilizatorul este logat, preluam id-urile utilizatorului pe care ii urmareste
            if (userId is not null)
            {
                followingUserIds = db.FollowRequests
                                     .Where(follow => follow.FollowerId == userId && follow.Status == FollowStatus.Accepted)
                                     .Select(follow => follow.FollowingId)
                                     .ToList();

                //il adauagam si pe el insusi pentru a-si vedea postarile in homepage
                followingUserIds.Add(userId);
            }

            //luam postarile utilizatorilor publici sau pe care ii urmarim (chiar daca ar avea cont privat)
            //in cazul in care utilizatorul nu este logat, cum followingUsersIds este o lista vida, va avea doar de verificat daca autorul postarii respective
            //are contul public
            var posts = db.Posts
                          .Include(post => post.Author)
                          .Where(post => followingUserIds.Contains(post.AuthorId) || post.Author.Public)
                          .OrderByDescending(post => post.Date)
                          .ToList();

            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View();
        }

        //feed-ul personalizat al utilizatorilor inregistrati sau administratori
        //contine postarile utilizatorlior pe care ii urmareste
        [Authorize(Roles="User,Administrator")]
        public IActionResult Feed()
        {
            var userId = _userManager.GetUserId(User);

            //luam persoanele pe care utilizatorul ii urmareste, mai exact verificam daca statusul cererii de urmarire este Accepted
            var followingUserIds = db.FollowRequests
                                     .Where(follow => follow.FollowerId == userId && follow.Status == FollowStatus.Accepted)
                                     .Select(follow => follow.FollowingId)
                                     .ToList();

            var posts = db.Posts
                          .Include(post => post.Author)
                          .Where(post => followingUserIds.Contains(post.AuthorId))
                          .OrderByDescending(post => post.Date)
                          .ToList();

            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            return View("Index");
        }

        //se afiseaza o singura postare in functie de ID-ul sau impreuna cu userul care a postat-o
        //[HttpGet] implicit

        //au acces la aceasta metoda atat utilizatorii inregistrati, cat si neinregistrati si administratorii
        [AllowAnonymous]
        public IActionResult Show(int Id)
        {
            Post? post = db.Posts
                           .Include(post => post.Author)
                           .Include(post => post.Comments)
                                 .ThenInclude(comment => comment.User)
                           .Where(post => post.PostId == Id)
                           .FirstOrDefault();

            if (post is null)
            {
                return NotFound();
            }

            //ordonam comentariile descrescator dupa data postarii
            post.Comments = post.Comments
                                .OrderByDescending(comment => comment.Date)
                                .ToList();

            //setam conditiile pentru afisarea butoanelor in viewul asociat postarii
            SetAccessRights();

            if(User.IsInRole("User") || User.IsInRole("Administrator"))
            {

                //preluam colectiile utilizatorului logat
                var myCollections = db.Collections
                                      .Where(collection => collection.UserId == _userManager.GetUserId(User))
                                      .ToList();

                ViewBag.UserCollections = myCollections;

                //extragem doar id-urile colectiilor pentru a verifica mai apoi colectiile in care este salvata posarea
                var myCollectionIds = myCollections
                                          .Select(collection => collection.CollectionId)
                                          .ToList();

                //pentru ca CollectionId este proprietate nullable in model, trebuie sa verificam daca exista si in caz afirmativ, ii vom prelua valoarea
                ViewBag.SavedCollectionIds = db.PostCollections
                                               .Where(postCollection => postCollection.PostId == Id 
                                                      && postCollection.CollectionId.HasValue 
                                                      && myCollectionIds.Contains(postCollection.CollectionId.Value))
                                               .Select(postCollection => postCollection.CollectionId.Value)
                                               .ToList();

            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(post);
        }

        //adaugarea unui comentariu asociat unei postari din baza de date
        //doar utilizatorii inregistrati si administratorii pot adauga comentarii

        [Authorize(Roles ="User,Administrator")]
        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            //data la care a fost postat comentariul
            comment.Date = DateTime.Now;

            //userul care a postat comentariul
            comment.UserId = _userManager.GetUserId(User);

            //daca comentariul trece validarile din model (dimenisunea continutului unui comentariu sa nu depaseasca un
            //anumit numar de caractere)
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Posts/Show/" + comment.PostId);
            }
            else
            {
                Post? post = db.Posts
                               .Include(post => post.Author)
                               .Include(post => post.Comments)
                                       .ThenInclude(comment => comment.User)
                               .Where(post => post.PostId == comment.PostId)
                               .FirstOrDefault();

                if(post is null)
                {
                    return NotFound();
                }

                SetAccessRights();

                return View(post);
            }
        }

        //se afiseaza formularul in care se vor completa datele unei posatri
        //doar utilizatorii autentificati (users) si administrtorii pot adauga articole in platforma
        //[HttpsGet] implicit

        [Authorize(Roles = "User,Administrator")]
        public IActionResult New()
        {
            Post post = new Post();

            return View(post);
        }

        //se adauga postarea in baza de date 
        //doar utilizatorii autentificati pot face postari in pltaforma sau administratorii
        
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
                //verificam extensia pentru imagine
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Fisierul trebuie sa fie imagine (jpg, jpeg, png, gif).");
                    return View(post);
                }

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

                //salvare fisier
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.ImageUrl));

                post.ImageUrl = databaseFileName;
            }

            if (hasVideo)
            {
                //verificam extensia pentru videoclip
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

            //nu am gasit postarea dupa id
            if (post == null)
            {
                return NotFound();
            }

            if (post.AuthorId == _userManager.GetUserId(User)
                || User.IsInRole("Administrator"))
            {
                return View(post);
            }

            //nu am acces la editarea postarii
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        //se adauga postarea in baza de date 
        //se verifica rolul utilizatorului care are dreptul sa editeze

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, Post requestPost, IFormFile? Image, IFormFile? Video, bool DeleteImage, bool DeleteVideo)
        {
            Post? post = db.Posts.Find(Id);

            //nu am gasit postarea dupa id
            if (post == null)
            {
                return NotFound();
            }

            if ((post.AuthorId == _userManager.GetUserId(User)) || User.IsInRole("Administrator"))
            {
                if (ModelState.IsValid)
                {
                    //campurile initiale ale postarii
                    string? initialTextContent = post.TextContent;
                    string? initialImageUrl = post.ImageUrl;
                    string? initialVideoUrl = post.VideoUrl;

                    post.TextContent = requestPost.TextContent;

                    //daca am ales o poza, o vom modifica
                    if (Image != null && Image.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Image", "Fișierul trebuie să fie imagine.");
                            return View(post);
                        }

                        // stergea vechea imagine fizic din wwwroot/images/posts DACA EXISTA 
                        if (!string.IsNullOrEmpty(post.ImageUrl))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        //generam un GUID pentru a stoca fisiere sub o denumire unica 
                        //conflicte pot aparea in cazul in care 2 utilizatori au salvat local un fisier cu acelasi nume, de exmeplu ,,poza.jpg"
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var storagePath = Path.Combine(_env.WebRootPath, "images", "posts");

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
                        post.ImageUrl = "/images/posts/" + uniqueFileName;
                    }

                    //daca am selectat totusi sa stergem imginea curenta, doar vom seta url ul pe null la acest pas
                    //stergerea fizica a imaginii din wwwroot se va face dupa verificarea finala: sa nu avem o postare goala
                    //in ruma editarii; in acest caz, vom pastra toate campurile initiale
                    else if (DeleteImage == true)
                    {
                        post.ImageUrl = null;
                    }

                    //daca am ales un video, o vom modifica
                    if (Video != null && Video.Length > 0)
                    {
                        var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".avi", ".mkv" };
                        var fileExtension = Path.GetExtension(Video.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Video", "Format video neacceptat.");
                            return View(post);
                        }

                        // stergea vechiul videoclip din wwwroot/videos/posts DACA EXISTA
                        if (!string.IsNullOrEmpty(post.VideoUrl))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, post.VideoUrl.TrimStart('/'));
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
                        post.VideoUrl = "/videos/posts/" + uniqueFileName;
                    }

                    //daca am selectat totusi sa stergem videoclipul curent, doar vom seta url ul pe null la acest pas
                    //stergerea fizica a videoclipului din wwwroot se va face dupa verificarea finala: sa nu avem o postare goala
                    //in ruma editarii; in acest caz, vom pastra toate campurile initiale
                    else if (DeleteVideo == true)
                    {
                        post.VideoUrl = null;
                    }

                    //daca am editat si am ajuns la o postare goala (toate cele 3 campuri TextContent, ImageUrl si VideoUrl sunt
                    //nule), nu pot face modiifcarea si voi mentine starea intiiala a postarii
                    //ne intoarcem in view cu valorile initiale
                    if (string.IsNullOrWhiteSpace(post.TextContent) && post.ImageUrl == null && post.VideoUrl == null)
                    {
                        post.TextContent = initialTextContent;
                        post.ImageUrl = initialImageUrl;
                        post.VideoUrl = initialVideoUrl;

                        ModelState.Clear();

                        ModelState.AddModelError(string.Empty, "Nu poți șterge tot conținutul! Modificările au fost anulate.");
                        return View(post);
                    }
                    //mai jos ajungem in cazul in care cel putin unul din campurile postarii este nenul
                    
                    // daca utilizatorul a vrut sa stearga imaginea (DeleteImage=true) și ea a fost setata pe null in obiect,
                    // dar exista o imagine initiala, atunci acum o stergem
                    if (post.ImageUrl == null && !string.IsNullOrEmpty(initialImageUrl) && DeleteImage == true)
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, initialImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // daca utilizatorul a vrut sa stearga videoclipul (DeletevVideo=true) și a fost setat pe null in obiect,
                    // dar exista un videoclip initial, atunci acum il stergem
                    if (post.VideoUrl == null && !string.IsNullOrEmpty(initialVideoUrl) && DeleteVideo == true)
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, initialVideoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    //salvam modificarile
                    await db.SaveChangesAsync();

                    TempData["message"] = "Postarea a fost modificată cu succes!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }

                //daca unul din campurle nu trece validarile, pastram ce am editat pana acum (referitor la textContent)
                else
                {
                    //avem nevoie sa reatribuim la requestPost imaginea si videoclipul postarii pentru a nu o pierde
                    requestPost.ImageUrl = post.ImageUrl;
                    requestPost.VideoUrl = post.VideoUrl;

                    return View(requestPost);
                }
            }

            //nu am acces la editarea postarii
            else
            {
                TempData["message"] = "Nu aveți dreptul să modificați această postare.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        //se sterge o postare din baza de date 
        //doar utilizatorii autentificati care au facut postarea respetiva o pot sterge

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Delete(int Id)
        {
            //nu trebuie sa stergem manual comentariile pentru ca am setat OnDeleteCascade, ceea ce inseamna ca daca sterg o postare, atunci comentariile 
            //asociate acesteia vor fi sterse automat
            Post? post = db.Posts
                           .Include(post => post.PostCollections)
                           .Where(post => post.PostId== Id)
                           .FirstOrDefault();

            //nu am gasit postarea dupa id
            if(post is null)
            {
                return NotFound();
            }
            else
            {
                if((post.AuthorId == _userManager.GetUserId(User))
                    || User.IsInRole("Administrator"))
                {
                    //stergem fizic din wwwroot/images/posts imaginea, daca exista
                    if (!string.IsNullOrEmpty(post.ImageUrl))
                    {
                        var imagePath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    //stergem fizic din wwwroot/videos/posts videocliupl, daca exista
                    if (!string.IsNullOrEmpty(post.VideoUrl))
                    {
                        var videoPath = Path.Combine(_env.WebRootPath, post.VideoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(videoPath))
                        {
                            System.IO.File.Delete(videoPath);
                        }
                    }

                    if (post.PostCollections != null && post.PostCollections.Any())
                    {
                        db.PostCollections.RemoveRange(post.PostCollections);
                    }

                    db.Posts.Remove(post);
                    db.SaveChanges();
                    TempData["message"] = "Postarea a fost stearsa cu succes.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }

                //nu am dreptul sa sterg postarea
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti o postare care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
        }


        //metoda aceasta se va ocupa atat de adaugarea unei postari intr-o colectie, cat si de eliminarea acesteia dintr-una
        //daca postarea deja exista in colectie, prin incercarea de a o adauga iar, o vom sterge din colectie
        //daca postarea nu exista in colectie, o vom aduaga
        [HttpPost]
        [Authorize(Roles ="User,Administrator")]
        public IActionResult AddToCollection([FromForm] PostCollection postCollection)
        {
            //daca trecem de validarile din model
            if (ModelState.IsValid)
            {

                //verificam daca avem deja postarea respectiva in colectie, adica daca exista o relatie intre postare si colectie in PostCollection
                var existingRelation = db.PostCollections
                                         .Where(pc => pc.PostId == postCollection.PostId)
                                         .Where(pc => pc.CollectionId == postCollection.CollectionId)
                                         .FirstOrDefault();

                //daca relatia exista, o vom sterge 
                if( existingRelation is not null)
                {
                    db.PostCollections.Remove(existingRelation);
                    db.SaveChanges();

                    TempData["message"] = "Postarea a fost eliminată din colecție";
                    TempData["messageType"] = "alert-warning";
                }

                //daca nu exista, o vom adauga
                else
                {
                    postCollection.AddedDate = DateTime.Now;

                    //adaugam asociarea intre postare si colectie
                    db.PostCollections.Add(postCollection);

                    //salvam modificarile in baza de date
                    db.SaveChanges();

                    //adaugam un mesaj de succes
                    TempData["message"] = "Postarea a fost adaugata in colectia selectata";
                    TempData["messageType"] = "alert-success";


                }
            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga postarea in colectie";
                TempData["messageType"] = "alert-danger";
            }

            //ne intoarcem la pagina postarii
            return Redirect("/Posts/Show/" + postCollection.PostId);
        }


        //conditiile de afisare pentru butoanele de editare si steregere
        //butoanele sunt aflate in view-uri
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
