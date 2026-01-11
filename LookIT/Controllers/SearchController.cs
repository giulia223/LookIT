using LookIT.Data;
using LookIT.Models;
using LookIT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LookIT.Controllers
{
    public class SearchController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public SearchController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string query)
        {
            var model = new SearchViewModel
            {
                SearchTerm = query
            };
            if (!string.IsNullOrWhiteSpace(query))
            {
                model.Users = await _userManager.Users
                    .Where(u => u.FullName.Contains(query) || u.UserName.Contains(query))
                    .ToListAsync();

            }
            return View(model);


        }


    }
}
