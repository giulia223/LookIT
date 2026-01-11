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
            //luam Id-ul utilizatorului logal
            var userId = _userManager.GetUserId(User);

            //facem o interrogare pentru a lua toate postarile unui utilizator si a le afisa
            //in profilulu sau
            var user = await _context.ApplicationUsers
                         .Include(user => user.Posts)
                         .FirstOrDefaultAsync( user => user.Id == userId);

            //daca userul nu exista
            if(user == null)
            {
                return NotFound();
            }

            //ordonam postarile utilizatorului descrescator dupa data adaugarii acestora
            user.Posts = user.Posts
                             .OrderByDescending(Post => Post.Date)
                             .ToList();

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

            ModelState.Remove(nameof(model.Id));
            ModelState.Remove(nameof(model.UserName));
            ModelState.Remove(nameof(model.NormalizedUserName));
            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.NormalizedEmail));
            ModelState.Remove(nameof(model.PasswordHash));
            ModelState.Remove(nameof(model.SecurityStamp));
            ModelState.Remove(nameof(model.ConcurrencyStamp));
            ModelState.Remove(nameof(model.PhoneNumber));

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
            var userPosts = await _context.Posts
                                  .Where(p => p.AuthorId == userId)
                                  .OrderByDescending(p => p.Date)
                                  .ToListAsync();

            
            


            ViewBag.IsOwner = isOwner;
            ViewBag.ShowFullProfile = showFullProfile;
            ViewBag.IsFollowing = isFollowing;
            ViewBag.IsPending = isPending;
            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
            ViewBag.UserPosts = userPosts;

            return View(targetUser);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FollowToggle(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge(); 

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

        // LISTA DE URMARITORI 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Followers(string userId)
        {
            var user = await _context.Users
                                     .FindAsync(userId);

            //aici am modififcat pentru ca pot vedrea lista de urmaritori si urmariri chiar daca nu am cont (logca este ca pot vizualiza 
            //profiluri chiar daca nu sunt autentificat)
            //if (user == null) return NotFound();

            ViewData["Title"] = $"Urmăritori - {user.FullName}";

            //cautam relațiile unde 'userId' este DESTINATARUL (FollowingId) și statusul e Accepted
            var followers = await _context.FollowRequests
                                          .Include(f => f.Follower) // Avem nevoie de datele celui care da follow
                                          .Where(f => f.FollowingId == userId && f.Status == FollowStatus.Accepted)
                                          .Select(f => f.Follower) // Selectam doar userii, nu obiectul cererii
                                          .ToListAsync();

            return View("UserList", followers); 
        }

        // LISTA DE URMARIRI 
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Following(string userId)
        {
            var user = await _context.Users
                                     .FindAsync(userId);

            //aici am modififcat pentru ca pot vedrea lista de urmaritori si urmariri chiar daca nu am cont (logca este ca pot vizualiza 
            //profiluri chiar daca nu sunt autentificat)
            //if (user == null) return NotFound();

            ViewData["Title"] = $"Urmăriri - {user.FullName}";

            //cautam relațiile unde 'userId' este EXPEDITORUL (FollowerId) si statusul e Accepted
            var following = await _context.FollowRequests
                                          .Include(f => f.Following) //avem nevoie de datele celui urmarit
                                          .Where(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted)
                                          .Select(f => f.Following)
                                          .ToListAsync();

            return View("UserList", following);
        }
    }
}
