using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LookIT.Models.ViewModels
{
        public class CreateProfileViewModel
        {
            [Required(ErrorMessage = "Numele este obligatoriu")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Descrierea este obligatorie")]
            public string Description { get; set; }

            [Required(ErrorMessage = "Poza este obligatorie")]
            public IFormFile ProfilePicture { get; set; }

            public bool Public { get; set; }
        }
    }
