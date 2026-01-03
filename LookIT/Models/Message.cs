using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        public DateTime Date { get; set; }
        //daca este postare de tip text
        [StringLength(5000, ErrorMessage = "Mesajul nu poate depăși {1} de caractere.")]
        public string? TextContent { get; set; }

        //daca este postare de tip imagine (stocam url ul)
        [StringLength(500, ErrorMessage = "Url-ul imaginii nu poate depasi {1} de caractere.")]
        public string? ImageUrl { get; set; }

        //daca este postare de tip video (stocam url ul)
        [StringLength(500, ErrorMessage = "Url-ul videoclipului nu poate depasi {1} de caractere.")]
        public string? VideoUrl { get; set; }

        public int GroupId { get; set; }
        public virtual Group? Group { get; set; }

        // Optional: mesajele pot fi anonime
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

    }
}
