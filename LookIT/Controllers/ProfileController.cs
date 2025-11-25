using LookIT.Models;
using LookIT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LookIT.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        //afisare profil user
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            return View(user);
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
    }
}
