using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class GroupMember
    {
        public DateTime Date { get; set; }

        public string Status { get; set; } = "Pending";

        // am pus UserId nvarchar(50) 
        //100 bytes + 4 bytes = 104 bytes < 900 bytes
        [Required(ErrorMessage ="Userul este obligatoriu")]
        public string MemberId { get; set; }

        public virtual ApplicationUser Member { get; set; }

        [Required(ErrorMessage ="Grupul este obligatoriu")]
        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

    }
}
