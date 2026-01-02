using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Like
    {
        //tabelul asociativ care face legatura intre Post si ApplicationUser
        //o postare poate fi apreciata de mai multi utilizatori, iar un utilizator poate aprecia mai multe postari

        [Key]
        public int LikeId { get; set; }
        public string? UserId { get; set; }
        public int PostId{ get; set; }

        //proprietate ne navigatie
        public virtual ApplicationUser? User { get; set; }

        //proprietate de navigatie
        public virtual Post? Post { get; set; }

    }
}
