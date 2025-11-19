using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Like
    {
        // am pus UserId nvarchar(50) 
        //100 bytes + 4 bytes = 104 bytes < 900 bytes
        [Required(ErrorMessage = "Userul este obligatoriu")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Postarea este obligatorie")]
        public int PostId{ get; set; }

        public virtual Post Post { get; set; }

    }
}
