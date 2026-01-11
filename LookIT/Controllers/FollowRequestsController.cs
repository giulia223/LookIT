using LookIT.Data;
using LookIT.Models;
using LookIT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Controllers
{
    public class FollowRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowRequestsController(UserManager<ApplicationUser> userManager,  ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //  Pagina care listeaza cererile primite
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var requests = await _context.FollowRequests
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == currentUser.Id && f.Status == FollowStatus.Pending)
                .ToListAsync();

            return View(requests);
        }

        //  Actiunea de Accept / Decline
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> HandleRequest(int requestId, string decision)
        {
            var request = await _context.FollowRequests.FindAsync(requestId);

            if (request == null) return NotFound();

            //Verific dacă cererea imi este adresata mie
            var currentUser = await _userManager.GetUserAsync(User);
            if (request.FollowingId != currentUser.Id) return Forbid();

            if (decision == "accept")
            {
                request.Status = FollowStatus.Accepted;
                _context.Update(request);
            }
            else if (decision == "decline")
            {
                _context.FollowRequests.Remove(request);
            }

            await _context.SaveChangesAsync();

            // Reincarc pagina ca sa dispara cererea procesata
            return RedirectToAction("MyRequests");
        }
    }
}
