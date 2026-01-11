using LookIT.Data;
using LookIT.Models;
using LookIT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Threading.Tasks;
using LookIT.Services;


namespace LookIT.Controllers
{
    public class PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env, IModerationService moderationService) : Controller
    {
        //injectarea depdendentelor in controller
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _env = env;
        private readonly IModerationService _moderationService = moderationService;


        //au acces la aceasta metoda atat utilizatorii inregistrati, cat si neinregistrati si administratorii
        //afisarea postarilor apartinand conturilor publice sau urmaritorilor (daca sunt conturi private)

        [AllowAnonymous]
        public IActionResult Index()
        {
            //afisam cate 4 postarii in pagina
            int _perPage = 4;
            
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

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            //verificam de fiecare data numarul postarilor totale
            int totalItems = posts.Count();

            //se preia pagina curenta din View-ul asociat, numarul paginii este valoarea parametrului page din ruta
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            //trebuie sa fortam ca pagina curenta sa fie 1, altfel cand dam next pentru prima data, ne vom intoarce tot pe rprima pagina
            if (currentPage == 0)
            {
                currentPage = 1;
            }

            //pentru prima pagina offsetul o sa fie 0, pentru pagina a doua va fi 4
            //asadar, offsetul este egal cu numarul de posari care au fost deja afisate pe paginile anterioarw
            var offset = 0;

            //se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            //se preiau postarile corespunzatoare pentru feicare pagina la care ne aflam in functie de offset
            var paginatedPosts = posts.Skip(offset).Take(_perPage).ToList();

            //preluam numarul ultimei pagini
            ViewBag.lastPage = (int)Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.CurrentPage = currentPage;

            //trimitem postarile cu ajutorul unui ViwBag  catre View0ul corespunzator
            ViewBag.PaginationBaseUrl = "/Posts/Index/?page=";
            ViewBag.Posts = paginatedPosts;

            return View();
        }

        //feed-ul personalizat al utilizatorilor inregistrati sau administratori
        //contine postarile utilizatorlior pe care ii urmareste

        [Authorize(Roles="User,Administrator")]
        public IActionResult Feed()
        {
            //afisam cate 4 postarii in pagina
            int _perPage = 4;

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

            //verificam de fiecare data numarul postarilor totale
            int totalItems = posts.Count();

            //se preia pagina curenta din View-ul asociat, numarul paginii este valoarea parametrului page din ruta
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            //trebuie sa fortam ca pagina curenta sa fie 1, altfel cand dam next pentru prima data, ne vom intoarce tot pe rprima pagina
            if (currentPage == 0)
            {
                currentPage = 1;
            }

            //pentru prima pagina offsetul o sa fie 0, pentru pagina a doua va fi 4
            //asadar, offsetul este egal cu numarul de posari care au fost deja afisate pe paginile anterioarw
            var offset = 0;

            //se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            //se preiau postarile corespunzatoare pentru feicare pagina la care ne aflam in functie de offset
            var paginatedPosts = posts.Skip(offset).Take(_perPage).ToList();

            //preluam numarul ultimei pagini
            ViewBag.lastPage = (int)Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.CurrentPage = currentPage;

            //trimitem postarile cu ajutorul unui ViwBag  catre View0ul corespunzator
            ViewBag.PaginationBaseUrl = "/Posts/Feed/?page=";
            ViewBag.Posts = paginatedPosts;


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
                           .Include(post => post.Likes)
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

                //preluam id-urile colectiilor utilizatorului in care se afla postarea
                ViewBag.SavedCollectionIds = db.PostCollections
                                               .Where(postCollection => postCollection.PostId == Id
                                                      && myCollectionIds.Contains(postCollection.CollectionId))
                                               .Select(postCollection => postCollection.CollectionId)
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
        public async Task<IActionResult> Show([FromForm] Comment comment)
        {
            //data la care a fost postat comentariul
            comment.Date = DateTime.Now;

            //userul care a postat comentariul
            comment.UserId = _userManager.GetUserId(User);

            //daca comentariul trece validarile din model (dimenisunea continutului unui comentariu sa nu depaseasca un
            //anumit numar de caractere)
            if (ModelState.IsValid)
            {
                //analizam continutul comentariului folosing OpenAI API

                var moderationResult = await _moderationService.CheckContentAsync(comment.Content);

                if (moderationResult.Success)
                {
                    if (moderationResult.IsFlagged is true)
                    {
                        TempData["message"] = $"Comentariul tau contine termeni nepotriviti, te rugam sa reformulezi. Motiv: {moderationResult.Reason}";
                        TempData["messageType"] = "alert-danger";

                        return RedirectToAction("Show", new { IDataTokensMetadata = comment.PostId });
                    }

                    comment.IsFlagged = moderationResult.IsFlagged;
                    comment.FlagCategory = "Safe";
                }

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
            //setarile initiale
            post.Date = DateTime.Now;
            post.AuthorId = _userManager.GetUserId(User);

            //valori booleaene pentru verificarea existentei componentelor unei postari
            bool hasText = !string.IsNullOrWhiteSpace(post.TextContent);
            bool hasImage = Image != null && Image.Length > 0;
            bool hasVideo = Video != null && Video.Length > 0;


            //verificam daca exista continut text
            if (!string.IsNullOrWhiteSpace(post.TextContent))
            {
                //apelam metoda CheckPostAsync din ModerationService 
                var moderationResult = await _moderationService.CheckPostAsync( post.TextContent );

                if (moderationResult.IsFlagged)
                {
                    //daca AI-ul zice ca e continut interzis (IsFlagged = true):

                    //afisam mesaj de eroare utilizatorului
                    ModelState.AddModelError("TextContent", $"Postarea ta contine termeni nepotriviti, te rugam sa reformulezi. Motiv: {moderationResult.Reason}");

                    //returnam View-ul cu datele introduse (ca sa nu le piarda), dar NU salvam in baza de date
                    return View(post);
                }
            }

            if (!hasText && !hasImage && !hasVideo)
            {
                ModelState.AddModelError(string.Empty, "Postarea nu poate fi goală!");
                return View(post);
            }

            if (hasImage)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension)) 
                { 
                    ModelState.AddModelError("Image", "Fisierul trebuie sa fie imagine."); 
                    return View(post); 
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "images", "posts");

                //daca nu exista folderul wwwroot/images/posts, il vom crea
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }

                var filePath = Path.Combine(storagePath, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) 
                { 
                    await Image.CopyToAsync(fileStream); 
                }

                ModelState.Remove(nameof(post.ImageUrl));
                post.ImageUrl = "/images/posts/" + uniqueFileName;
            }

            if (hasVideo)
            {
                var allowedExtensions = new[] { ".mp4", ".mov", ".webm", ".avi", ".mkv" };
                var fileExtension = Path.GetExtension(Video.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension)) 
                { 
                    ModelState.AddModelError("Video", "Fisierul trebuie sa fie video."); 
                    return View(post); 
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var storagePath = Path.Combine(_env.WebRootPath, "videos", "posts");

                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }

                var filePath = Path.Combine(storagePath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create)) 
                { 
                    await Video.CopyToAsync(fileStream); 
                }

                ModelState.Remove(nameof(post.VideoUrl));
                post.VideoUrl = "/videos/posts/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                await db.SaveChangesAsync();
                TempData["message"] = "Postarea a fost adaugata cu succes!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(post);
            }
        }

        //se editeaza o postare in baza de date
        //se afiseaza formularul impreuna cu datele aferente postarii
        //doar userii al caror postari le apartin pot edita postarile
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

            // nu am gasit postarea dupa id
            if (post == null)
            {
                return NotFound();
            }

            if ((post.AuthorId == _userManager.GetUserId(User)) || User.IsInRole("Administrator"))
            {
                if (ModelState.IsValid)
                {
                    // campurile initiale ale postarii
                    string? initialTextContent = post.TextContent;
                    string? initialImageUrl = post.ImageUrl;
                    string? initialVideoUrl = post.VideoUrl;

                    // Actualizam textul din input
                    post.TextContent = requestPost.TextContent;

                    if (!string.IsNullOrWhiteSpace(post.TextContent))
                    {
                        //apelam serviciul de moderare pentru textul editat
                        var moderationResult = await _moderationService.CheckPostAsync(post.TextContent);

                        if (moderationResult.IsFlagged)
                        {
                            //daca AI-ul zice ca e continut interzis:
                            ModelState.AddModelError("TextContent", $"Postarea ta contine termeni nepotriviti, te rugam sa reformulezi. Motiv: {moderationResult.Reason}");

                            //resetam textul la cel original pentru ca modificarea este ilegala
                            post.TextContent = initialTextContent;

                            //trebuie sa populam requestPost cu URL-urile existente ca sa nu dispara imaginile din View
                            requestPost.ImageUrl = post.ImageUrl;
                            requestPost.VideoUrl = post.VideoUrl;

                            return View(requestPost);
                        }
                    }

                    // daca am ales o poza, o vom modifica
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

                        // generam un GUID
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var storagePath = Path.Combine(_env.WebRootPath, "images", "posts");

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

                    // daca am selectat totusi sa stergem imginea curenta
                    else if (DeleteImage == true)
                    {
                        post.ImageUrl = null;
                    }

                    // daca am ales un video, o vom modifica
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

                    else if (DeleteVideo == true)
                    {
                        post.VideoUrl = null;
                    }

                    //verificare postare goala
                    if (string.IsNullOrWhiteSpace(post.TextContent) && post.ImageUrl == null && post.VideoUrl == null)
                    {
                        post.TextContent = initialTextContent;
                        post.ImageUrl = initialImageUrl;
                        post.VideoUrl = initialVideoUrl;

                        ModelState.Clear();
                        ModelState.AddModelError(string.Empty, "Nu poți șterge tot conținutul! Modificările au fost anulate.");
                        return View(post);
                    }

                    //steregrea fizica a fisierelor vechi (daca este cazul)
                    if (post.ImageUrl == null && !string.IsNullOrEmpty(initialImageUrl) && DeleteImage == true)
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, initialImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

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
                    requestPost.ImageUrl = post.ImageUrl;
                    requestPost.VideoUrl = post.VideoUrl;
                    return View(requestPost);
                }
            }
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


        [HttpPost]
        [Authorize(Roles ="User,Administrator")]
        public IActionResult RemoveFromCollection(int postId, int collectionId)
        {
            var postCollection = db.PostCollections
                                    .FirstOrDefault(pc => pc.PostId == postId && pc.CollectionId == collectionId);

            //daca exista relatia, o vom sterge
            if(postCollection is not null)
            {
                var collection = db.Collections.Find(collectionId);
                var currentUserId = _userManager.GetUserId(User);

                if((collection.UserId == currentUserId || User.IsInRole("Administrator")) && collection is not null)
                {
                    db.PostCollections.Remove(postCollection);
                    db.SaveChanges();

                    TempData["message"] = "Postarea a fost eliminată din colecție.";
                    TempData["messageType"] = "alert-success";
                }
                else
                {
                    TempData["message"] = "Nu aveți dreptul să modificați această colecție.";
                    TempData["messageType"] = "alert-danger";
                }
            }

            return RedirectToAction("Show", "Collections", new { id = collectionId });
        }


        //aceasta metoda se va ocupa atat de aprecierea unei postari, cat si de scoaterea acesteia de la apreciere
        //daca postarea este deja apreciata, prin incercarea de a o aprecia iar, vom scoate like-ul asociat
        //daca postarea nu este apeciata, atunci se va aduga o relatie intre Post si User in tabela asociativa Likes

        [HttpPost]
        [Authorize(Roles ="User, Administrator")]
        public IActionResult LikePost([FromForm] int Id)
        {
            //verificam daca exista deja o relatie de apreciere intre utilizatorul logal si postare
            var existingRelation = db.Likes
                                     .FirstOrDefault(like => like.PostId == Id
                                                             && like.UserId == _userManager.GetUserId(User));

            //variabila pentru a determina daca am dat like postarii sau nu
            bool isLikedNow;

            //daca exista relatia, vom sterge like ul
            if (existingRelation is not null)
            {
                db.Likes.Remove(existingRelation);
                isLikedNow = false;

                TempData["message"] = "Postarea a fost eliminata din apreceri";
                TempData["messageType"] = "alert-warning";
            }

            //daca relatia nu exista, o vom adauga
            else
            {
                var newLike = new Like
                {
                    PostId = Id,
                    UserId = _userManager.GetUserId(User)
                };
                isLikedNow = true;

                db.Likes.Add(newLike);

                TempData["message"] = "Postarea a fost apreciata";
                TempData["messageType"] = "alert-success";
            }
            db.SaveChanges();
            int newCount = db.Likes
                             .Count(like => like.PostId == Id);

            return Json(new { success = true, isLiked = isLikedNow, count = newCount });
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
