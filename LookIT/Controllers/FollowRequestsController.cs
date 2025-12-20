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

        // 1. Pagina care listează cererile primite
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Căutăm în tabela FollowRequests cererile unde:
            // - Destinatarul (FollowingId) sunt EU
            // - Statusul este PENDING
            // - Includem și datele celui care a trimis (Follower) ca să-i vedem numele/poza
            var requests = await _context.FollowRequests
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == currentUser.Id && f.Status == FollowStatus.Pending)
                .ToListAsync();

            return View(requests);
        }

        // 2. Acțiunea de Accept / Decline
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> HandleRequest(int requestId, string decision)
        {
            var request = await _context.FollowRequests.FindAsync(requestId);

            if (request == null) return NotFound();

            // Securitate: Verificăm dacă cererea îmi este adresată mie
            var currentUser = await _userManager.GetUserAsync(User);
            if (request.FollowingId != currentUser.Id) return Forbid();

            if (decision == "accept")
            {
                // Modificăm statusul în Accepted -> Devine urmăritor oficial
                request.Status = FollowStatus.Accepted;
                _context.Update(request);
            }
            else if (decision == "decline")
            {
                // Ștergem cererea definitiv
                _context.FollowRequests.Remove(request);
            }

            await _context.SaveChangesAsync();

            // Reîncărcăm pagina ca să dispară cererea procesată
            return RedirectToAction("MyRequests");
        }
    }
}
