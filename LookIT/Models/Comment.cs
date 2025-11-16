using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        public DateTime Date {  get; set; }
        public string Content { get; set; }
        [Required(ErrorMessage ="Autorul comentariului este obligatoriu")]
        public string UserId { get; set; }
        [Required(ErrorMessage ="Autorul comentariului este obligatoriu")]
        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Postarea este obligatorie")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Postarea este obligatorie")]
        public virtual Post Post { get; set; }


    }
}
