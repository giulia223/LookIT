using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LookIT.Controllers
{

    public class GroupsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        //afisare toate grupurile
        public IActionResult Index()
        {

            int _perPage = 4;
            var groups = _context.Groups.Include(g => g.Messages)
                                    .Include(g => g.Moderator).ToList();

            //verificam de fiecare data numarul postarilor totale
            int totalItems = groups.Count();

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
            var paginatedGroups = groups.Skip(offset).Take(_perPage).ToList();

            //preluam numarul ultimei pagini
            ViewBag.lastPage = (int)Math.Ceiling((double)totalItems / _perPage);
            ViewBag.CurrentPage = currentPage;

            //trimitem postarile cu ajutorul unui ViwBag  catre View0ul corespunzator
            ViewBag.PaginationBaseUrl = "/Groups/Index/?page=";
            ViewBag.Groups = paginatedGroups;

            return View();

        }

        //afisare detalii grup
        public IActionResult Show(int Id)
        {
            var grp = _context.Groups
                .Where(g => g.GroupId == Id)
                .Include(g => g.Moderator)
                .Include(g => g.Messages)
                .Include(g => g.Members)
                .ThenInclude(g => g.Member)
                .FirstOrDefault();
            if (grp == null)
            {
                return NotFound();
            }
            SetAccessRights();
            ViewBag.GroupDetails = grp;
            ViewBag.Messages = grp.Messages.OrderByDescending(m => m.Date).ToList();
            ViewBag.ActiveMembersCount = grp.Members.Count(m => m.Status != "Pending");

            // 2. Facem o listă doar cu cererile în așteptare
            ViewBag.PendingRequests = grp.Members.Where(m => m.Status == "Pending").ToList();

            // 3. Facem o listă cu membrii deja acceptați (pentru lista de membri)
            ViewBag.AcceptedMembers = grp.Members.Where(m => m.Status != "Pending").ToList();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var currentUserId = _userManager.GetUserId(User);

            // Găsim intrarea GroupMember pentru userul curent și grupul dat
            var userGroupEntry = grp.Members.FirstOrDefault(m => m.MemberId == currentUserId);

            string statusForView;

            if (userGroupEntry == null)
            {
                statusForView = "None"; // Nu este membru și nu a trimis cerere
            }
            else
            {
                // Trimitem statusul exact (ex: "Pending", "Moderator", "Accepted")
                statusForView = userGroupEntry.Status;
            }

            ViewBag.GroupDetails = grp;
            ViewBag.UserGroupStatus = statusForView; // Transmiterea rezultatului către View
            return View();
        }

        //JOIN GROUP
        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult JoinGroup([FromForm] GroupMember gpm)
        {
            gpm.Date = DateTime.Now;
            gpm.MemberId = _userManager.GetUserId(User);
            // Daca modelul este valid

            if (ModelState.IsValid)
            {
                // Verificam daca avem deja in colectie
                if (_context.GroupMembers
                    .Where(ab => ab.GroupId == gpm.GroupId)
                    .Where(ab => ab.MemberId == gpm.MemberId)
                    .Count() > 0 && gpm.Status == "Accepted")
                {
                    TempData["message"] = "Acest grup este deja adaugat in lista ta de grupuri";
                    TempData["messageType"] = "alert-danger";
                }
                else if (_context.GroupMembers
                    .Where(ab => ab.GroupId == gpm.GroupId)
                    .Where(ab => ab.MemberId == gpm.MemberId)
                    .Count() > 0 &&  gpm.Status == "Pending")
                {
                    TempData["message"] = "Cererea de join este in asteptare";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    var newGroupMember = new GroupMember
                    {
                        GroupId = gpm.GroupId,
                        MemberId = gpm.MemberId,
                        Date = gpm.Date,
                        Status = "Pending" // Setează explicit la Pending
                    };

                    _context.GroupMembers.Add(newGroupMember);
                    _context.SaveChanges();

                    TempData["message"] = "Cererea de join a fost trimisă și așteaptă aprobarea.";
                    TempData["messageType"] = "alert-success";  
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut realiza cererea";
                TempData["messageType"] = "alert-danger";
            }

            // Ne intoarcem la pagina groupului
            return RedirectToAction("Show", new {Id = gpm.GroupId});
        }

        //CREARE GRUP
        [Authorize(Roles = "User")]
        public IActionResult New()
        {
            Group gp = new Group();
          
            return View(gp);
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult New(Group gp)
        {
            // preluam Id-ul utilizatorului care face grupul
            gp.ModeratorId = _userManager.GetUserId(User);
            gp.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Groups.Add(gp);
                _context.SaveChanges();
                var groupMember = new GroupMember
                {
                    GroupId = gp.GroupId,
                    Date = gp.Date,
                    MemberId = gp.ModeratorId, 
                    Status = "moderator"
                };

                _context.GroupMembers.Add(groupMember);
                _context.SaveChanges();
                TempData["message"] = "Grupul a fost creat cu succes";
                TempData["messageType"] = "alert-success";
               
                return RedirectToAction("Show", "GroupMembers");
            }

            else
            {
                TempData["message"] = "Eroare la crearea grupului. Verificati campurile.";  
                TempData["messageType"] = "alert-danger";
                ViewBag.Message = TempData["message"].ToString();
                return View(gp);
            }
        }

        //editare grup

        [Authorize(Roles = "User,Administrator")]
        public IActionResult Edit(int Id)
        {
            Group? grp = _context.Groups.Find(Id);
            if (grp == null)
            {
                return NotFound();
            }
            if (grp.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Administrator")) 
            {
                return View(grp);
                SetAccessRights();
            }
            else
            {

                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui grup care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", Id);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Edit(int Id, Group group)
        {
            Group? grp = _context.Groups.Find(Id);
            if (grp == null)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (grp.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))
                    {
                        SetAccessRights();
                        grp.Description = group.Description;
                        grp.GroupName = group.GroupName;
                        TempData["message"] = "Grupul a fost modificat";
                        TempData["messageType"] = "alert-success";
                        _context.SaveChanges();
                        return RedirectToAction("Show", "Groups", new {Id = grp.GroupId});
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Show", "Groups", new { Id = grp.GroupId });
                    }
                }
                else
                {
                    TempData["message"] = "Eroare la modificarea grupului. Verificati campurile.";
                    TempData["messageType"] = "alert-danger";
                    ViewBag.Message = TempData["message"].ToString();
                    return View(grp);
                }
            }
           
        }

        //stergere grup 
        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public IActionResult Delete(int Id)
        {
            var grp = _context.Groups.Find(Id);
            if (grp != null && (grp.ModeratorId == _userManager.GetUserId(User) || User.IsInRole("Administrator"))) {

                var groupMessages = _context.Messages.Where(m => m.GroupId == Id).ToList();
                _context.Messages.RemoveRange(groupMessages);

                var groupMembers = _context.GroupMembers.Where(gm => gm.GroupId == Id).ToList();
                _context.GroupMembers.RemoveRange(groupMembers);
              
            }
            if (grp == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(grp);
            _context.SaveChanges();

            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index", "Groups");
            }

            return RedirectToAction("Show", "GroupMembers");

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
