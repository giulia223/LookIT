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
        public IActionResult Index()
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            if (User.IsInRole("User"))
            {
                //iau colectiile proprii
                var collections = db.Collections
                                    .Include(collection => collection.User)
                                    .Where(collection => collection.UserId == _userManager.GetUserId(User))
                                    .ToList();


                ViewBag.Collections = collections;
                return View();
            }
            else
            {
                if (User.IsInRole("Administrator"))
                {
                    //daca sunt administrator, voi putea vedea toate colectiile din platforma
                    var collections = db.Collections
                                        .Include(collection => collection.User)
                                        .ToList();

                    ViewBag.Collections = collections;
                    return View();

                }
                else
                {
                    TempData["message"] = "Nu aveti drepturi asupra colectiei.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
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
        public IActionResult New()
        {

            return View();
        }

        //adaugarea colectiei in baza de date
        [HttpPost]
        [Authorize(Roles ="User,Administrator")]
        public IActionResult New(Collection collection)
        {
            collection.UserId = _userManager.GetUserId(User);
            collection.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Collections.Add(collection);
                db.SaveChanges();
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
            return View(collection);
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


    }
}
