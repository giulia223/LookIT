using LookIT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LookIT.Controllers
{
    public class RegisteredUserController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        public RegisteredUserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

       // [Authorize]
       
    }
}
