using LookIT.Data;
using LookIT.Models;
using LookIT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _userManager = userManager;
            _env = env;
            _context = context;
        }

        //afisare profil user
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            return RedirectToAction("Details", new { userId = user.Id });
        }


        [AllowAnonymous]
        //creare profil user
        public IActionResult CreateProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Register", "???");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateProfile(CreateProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            // mapare date
            user.FullName = model.FullName;
            user.Description = model.Description;
            user.Public = model.Public;

            // poza
            if (model.ProfilePicture != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "images/profile");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = "/images/profile/" + fileName;
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Profile");
        }


        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser model, IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            user.FullName = model.FullName;
            user.Description = model.Description;
            user.Public = model.Public;


            if (profilePicture != null && profilePicture.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "images/profile");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);

                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = "/images/profile/" + fileName;
            }
            await _userManager.UpdateAsync(user);


            return RedirectToAction("Index");

        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var targetUser = await _context.Users
                //.Include(u => u.Articles) 
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (targetUser == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            

            bool isOwner = currentUser != null && currentUser.Id == targetUser.Id;

            
            var followRelationship = currentUser != null
                ? await _context.FollowRequests.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == targetUser.Id)
                : null;

            bool isFollowing = followRelationship != null && followRelationship.Status == FollowStatus.Accepted;
            bool isPending = followRelationship != null && followRelationship.Status == FollowStatus.Pending;

            
            bool showFullProfile = targetUser.Public || isOwner || isFollowing;

          
            var followersCount = await _context.FollowRequests.CountAsync(f => f.FollowingId == targetUser.Id && f.Status == FollowStatus.Accepted);
            var followingCount = await _context.FollowRequests.CountAsync(f => f.FollowerId == targetUser.Id && f.Status == FollowStatus.Accepted);
            //var postsCount = targetUser.Articles?.Count ?? 0;

            
            ViewBag.IsOwner = isOwner;
            ViewBag.ShowFullProfile = showFullProfile;
            ViewBag.IsFollowing = isFollowing;
            ViewBag.IsPending = isPending;
            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
            //ViewBag.PostsCount = postsCount;

            return View(targetUser);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FollowToggle(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge(); // Te trimite la login  ???

            // Verificam daca exista deja o relatie
            var existingFollow = await _context.FollowRequests
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == userId);

            if (existingFollow != null)
            {
                // Daca exista, inseamna ca dam UNFOLLOW (stergem relatia)
                _context.FollowRequests.Remove(existingFollow);
            }
            else
            {
                // Daca nu exista, cream cererea
                var targetUser = await _context.Users.FindAsync(userId);
                if (targetUser == null) return NotFound();

                var newFollow = new FollowRequest
                {
                    FollowerId = currentUser.Id,
                    FollowingId = userId,
                    // Daca e public -> Accepted direct. Daca e privat -> Pending.
                    Status = targetUser.Public ? FollowStatus.Accepted : FollowStatus.Pending
                };
                _context.FollowRequests.Add(newFollow);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { userId = userId });
        }

        // LISTA DE URMĂRITORI (Cine mă urmărește pe mine)
        [HttpGet]
        public async Task<IActionResult> Followers(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            ViewData["Title"] = $"Urmăritori - {user.FullName}";

            // Căutăm relațiile unde 'userId' este DESTINATARUL (FollowingId) și statusul e Accepted
            var followers = await _context.FollowRequests
                .Include(f => f.Follower) // Avem nevoie de datele celui care dă follow
                .Where(f => f.FollowingId == userId && f.Status == FollowStatus.Accepted)
                .Select(f => f.Follower) // Selectăm doar userii, nu obiectul cererii
                .ToListAsync();

            return View("UserList", followers); // Refolosim un View comun "UserList"
        }

        // LISTA DE URMĂRIRI (Pe cine urmăresc eu)
        [HttpGet]
        public async Task<IActionResult> Following(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            ViewData["Title"] = $"Urmăriri - {user.FullName}";

            // Căutăm relațiile unde 'userId' este EXPEDITORUL (FollowerId) și statusul e Accepted
            var following = await _context.FollowRequests
                .Include(f => f.Following) // Avem nevoie de datele celui urmărit
                .Where(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted)
                .Select(f => f.Following)
                .ToListAsync();

            return View("UserList", following);
        }
    }
}
