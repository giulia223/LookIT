using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        //nu punem Required pentru ca atunci va esua la validarile din model in momentul adaugarii postarii
        public DateTime Date { get; set; }

        //daca este postare de tip text
        [StringLength(5000, ErrorMessage = "Postarea nu poate depăși {1} de caractere.")]
        public string? TextContent { get; set; }

        //daca este postare de tip imagine (stocam url ul)
        [StringLength(500, ErrorMessage ="Url-ul imaginii nu poate depasi {1} de caractere.")]
        public string? ImageUrl { get; set; }

        //daca este postare de tip video (stocam url ul)
        [StringLength(500, ErrorMessage ="Url-ul videoclipului nu poate depasi {1} de caractere.")]
        public string? VideoUrl { get; set; }

        //nu punem Required pentru ca atunci va esua la validarile din model in momentul adaugarii postarii
        //cheie externa (FK): o postare este facuta de catre un user
        public string? AuthorId { get; set; }

        //nu punem Required pentru ca atunci va esua la validarile din model in momentul adaugarii postarii
        //proprietatea de nevigatie: o postare este facuta de catre un user
        public virtual ApplicationUser? Author { get; set; }

        //o postare poate avea o colectie de comentarii
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        //o postare este apreciata de mai multe persone/utilizatori
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        //relatia many-to-many dintre Post si Collection
        public virtual ICollection<PostCollection> PostCollections { get; set; } = new List<PostCollection>();



    }
}
