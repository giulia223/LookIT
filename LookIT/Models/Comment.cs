using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        //nu punem Required pentru ca atunci va esua la validarile din model in momentul adaugarii comentariului
        public DateTime Date {  get; set; }

        public DateTime? DateModified { get; set; }

        [Required(ErrorMessage ="Continutul comentariului nu poate fi gol.")]
        [StringLength(1000, ErrorMessage ="Continutul comentariului nu poate depasi {1} de caractere.")]
        public string? Content { get; set; }

        //cheie externa (FK) - un comentariu este postat de catre un user
        //nu punem required pentru ca atunci va esua la validarile din model in momentul adaugarii comentariului

        public string? UserId { get; set; }

        //cheie externa (FK) - un comentariu este postat de catre un user
        public int PostId { get; set; }

        //proprietatea de navigatie - un comentariu este postat de catre un user
        public virtual ApplicationUser? User { get; set; }

        //proprietatea de navigatie - un comentariu apartine unei postari
        public virtual Post? Post { get; set; }

        //campuri noi pentru analiza comentariului
        //indica daca AI-ul a marcat comentariul ca fiind nepotrivit
        //true = continut perisuclos/interizis -> se blocheaza publicarea lui
        //false = continut sigur
        public bool? IsFlagged  { get; set; }

        //motivul pentru care a fost marcat ca si nesigur, ex : hate speech, violcenta, sexual
        public string? FlagCategory { get; set; }

        //data si ora la care s-a efectuat analiza
        public DateTime? ModeratedAt { get; set; }
    }
}
