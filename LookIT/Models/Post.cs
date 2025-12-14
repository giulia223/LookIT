using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public DateTime Date { get; set; }

        //daca este postare de tip text
        [StringLength(5000, ErrorMessage = "Postarea nu poate depăși {1} de caractere.")]
        public string? TextContent { get; set; }

        //daca este postare de tip imagine (stocam url ul)
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        //daca este postare de tip video (stocam url ul)
        [StringLength(500)]
        public string? VideoUrl { get; set; }

        //cheie externa (FK): o postare este facuta de catre un user
        public string? AuthorId { get; set; }

        //proprietatea de nevigatie: o postare este facuta de catre un user
        public virtual ApplicationUser? Author { get; set; }

        //o postare poate avea o colectie de comentarii
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        public virtual ICollection<Save> Saves { get; set; } = new List<Save>();

    }
}
