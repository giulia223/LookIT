using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        public DateTime Date { get; set; }
        [Required(ErrorMessage ="Continutul mesajului nu poate fi gol")]
        public string Content { get; set; }
        [Required]
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

        // Optional: mesajele pot fi anonime
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

    }
}
