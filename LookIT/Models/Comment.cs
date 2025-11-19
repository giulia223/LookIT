using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public DateTime Date {  get; set; }

        [Required(ErrorMessage ="Continutul comentariului nu poate fi gol")]
        [StringLength(1000, ErrorMessage ="Continutul comentariului nu poate depasi {1} de caractere")]

        public string Content { get; set; }
        [Required(ErrorMessage ="Autorul comentariului este obligatoriu")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Postarea este obligatorie")]
        public int PostId { get; set; }

        public virtual Post Post { get; set; }

    }
}
