using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LookIT.Controllers
{
    public class CollectionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IWebHostEnvironment _env = env;

        //afisarea tuturor colectiilor unui utilizator

        [Authorize(Roles ="User,Administrator")]
        public IActionResult Index()
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            //iau colectiile proprii
            var collections = db.Collections
                                .Include(collection => collection.User)
                                .Where(collection => collection.UserId == _userManager.GetUserId(User))
                                //punem colectia default "All Posts" pe prima pozitie
                                .OrderByDescending(collection => collection.Name == "All Posts")
                                //iar pe restul le vom ordona descrescator dupa data crearii 
                                .ThenByDescending(collection => collection.CreationDate)
                                .ToList();


            ViewBag.Collections = collections;
            return View();
             
        }

        //afisarea postarilor pe care utilizatorul le-a salvat in colectia sa

        [Authorize(Roles ="User,Administrator")]
        public IActionResult Show(int Id)
        {
            var collection = db.Collections
                               .Include(collection => collection.User)
                                .Include(collection => collection.PostCollections) 
                                    .ThenInclude(postCollection => postCollection.Post) 
                                         .ThenInclude(post => post.Author) 
                                .FirstOrDefault(collection => collection.CollectionId == Id);

            if (collection == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // verificam daca userul are voie sa vada colectia
            string currentUserId = _userManager.GetUserId(User);

            if (collection.UserId != currentUserId && !User.IsInRole("Administrator"))
            {
                TempData["message"] = "Nu aveți dreptul să vedeți această colecție.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            SetAccessRights();

            return View(collection);
        }

        //formularul in care se completeaza detele unei colectii
        //avem [HttpGet] implicit

        [Authorize(Roles="User,Administrator")]
        public IActionResult New(int? postId)
        {
            var collection = new Collection();
            
            //verificam daca avem parametrul optinal de salvare (adica daca venim de pe calea unei postari in crearea colectiei)
            //verificam cu o variabila in ViewBag in care dintre cazuri ne incadram

            //venim de pe pagina unei postari, cand vrem sa o salvam si selectam ,,Colectie noua"
            if(postId.HasValue)
            {
                ViewBag.PostToSave = postId;
            }

            //suntem in pagina propriilor colectii si vrem sa adugam o colectie goala
            else
            {
                ViewBag.PostToSave = null;
            }
            return View(collection);
        }

        //adaugarea colectiei in baza de date

        [HttpPost]
        [Authorize(Roles ="User,Administrator")]
        public IActionResult New(Collection collection, int? PostToSave)
        {
            collection.UserId = _userManager.GetUserId(User);
            collection.CreationDate = DateTime.Now;

            
            if(db.Collections
                .Any(c=> c.UserId == collection.UserId && c.Name == collection.Name))
            {
                ModelState.AddModelError("Name", "Exista deja o colectie cu acest nume.");
            }

            //daca trece din validarile din model
            if (ModelState.IsValid)
            {
                db.Collections.Add(collection);
                db.SaveChanges();

                if (PostToSave.HasValue)
                {
                    var save = new PostCollection
                    {
                        PostId = PostToSave.Value,
                        CollectionId = collection.CollectionId,
                        AddedDate = DateTime.Now
                    };

                    db.PostCollections.Add(save);
                    db.SaveChanges();

                    return RedirectToAction("Show", "Posts", new { id = PostToSave.Value });
                }
               
                TempData["message"] = "Colectia a fost adaugata";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(collection);
            }
        }

        //se editeaza o colectie, mai exact numele/titlul acesteia
        //[HttpGet] implicit
        public IActionResult Edit(int Id)
        {
            Collection? collection = db.Collections.Find(Id);

            if(collection is null)
            {
                return NotFound();
            }

            if(User.IsInRole("Administrator") || _userManager.GetUserId(User) == collection.UserId)
            {
                if(collection.Name == "All Posts")
                {
                    TempData["message"] = "Nu puteți edita colecția implicită 'All Posts'.";
                    TempData["messageType"] = "alert-warning";
                    return RedirectToAction("Index");
                }
                return View(collection);

            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati o colectie care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Edit(int Id, Collection requestCollection)
        {
            Collection? collection = db.Collections.Find(Id);

            if(collection is null)
            {
                return NotFound();
            }

            if(User.IsInRole("Administrator") || _userManager.GetUserId(User) == collection.UserId)
            {
                if(collection.Name == "All Posts")
                {
                    TempData["message"] = "Nu puteți edita colecția implicită 'All Posts'.";
                    TempData["messageType"] = "alert-warning";
                    return RedirectToAction("Index");
                }

                //daca trece de validarile din model, adica obligativitatea titlului
                if (ModelState.IsValid)
                {
                    collection.Name = requestCollection.Name;
                    db.SaveChanges();
                    TempData["message"] = "Colectia a fost editata!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestCollection);
                }
            }
            //nu am dreptul sa editez colectia
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati o colectie care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }


        //se sterge o colectie a unui utilizator din baza de date 
        //doar utilizatorii autentificati care au creat respectiva colectie o pot sterge, sau administratorii

       
        [HttpPost]
        [Authorize(Roles="User,Administrator")]
        public IActionResult Delete(int Id)
        {
            Collection? collection = db.Collections
                                       .Where(collection => collection.CollectionId == Id)
                                       .FirstOrDefault();

            //nu am gasit colectia dupa Id
            if(collection is null)
            {
                return NotFound();
            }
            else
            {
                if (User.IsInRole("Administrator")
                    || collection.UserId == _userManager.GetUserId(User))
                {

                    //verificam daca este colectia predefinita, All Posts, caz in care nu avem permisiunea de a o sterge
                    if (collection.Name == "All Posts")
                    {
                        TempData["message"] = "Nu puteți șterge colecția implicită 'All Posts'.";
                        TempData["messageType"] = "alert-warning";
                        return RedirectToAction("Index");
                    }

                    //pentru ca am pus restrict intre relatia dintre colectii si postari, trebuie sa stergem manual legaturile
                    //dintre acestea
                    var postCollections = db.PostCollections
                                            .Where(pc => pc.CollectionId == Id)
                                            .ToList();

                    if (postCollections.Any())
                    {
                        db.PostCollections.RemoveRange(postCollections);
                    }

                    db.Collections.Remove(collection);
                    db.SaveChanges();
                    TempData["message"] = "Colectia a fost stearsa cu succes.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }

                //nu am dreptul sa sterg colectia
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti o colectie care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }

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

        //metoda care verifica baza de date si returneaza sub forma unui json daca userul are deja colectia cu numele respectiv
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyUniqueName(string name)
        {
            var userId = _userManager.GetUserId(User);

            if(db.Collections
                .Any(collection => collection.Name == name && collection.UserId == userId))
            {
                return Json($"Există deja o colecție cu acest nume.");
            }

            return Json(true);
        }

    }
}
