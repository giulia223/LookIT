using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required(ErrorMessage ="Data este obligatorie")]
        public DateTime Date { get; set; }
        public string? TextContent { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }

        [Required(ErrorMessage ="Autorul postarii este obligatoriu")]
        public string AuthorId { get; set; }
        [Required(ErrorMessage = "Autorul postarii este obligatoriu")]
        public virtual ApplicationUser Author { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Like> Likes { get; set; }

        // NU stoca fișierele (imagini, video) direct în baza de date!
        // Stochează doar calea (URL-ul) către fișierul urcat pe server 
        // (de ex., în wwwroot/uploads) sau în cloud (Azure Blob Storage, S3 etc.)
    }
}
