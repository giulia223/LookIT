using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class PostCollection
    {
        //tabelul care face legatura intre Post si Collection
        //o postare poate fi salvata in mai multe colectii, iar o colectie
        //contine mai multe postari

        [Key]
        public int Id { get; set; }
        public int PostId { get; set; }
        public int CollectionId { get; set; }

        //proprietate de navigatie
        public virtual Post? Post { get; set; }

        //proprietate de navigatie
        public virtual Collection? Collection { get; set; }

        //data la care o postare a fost salvata intr-o anumita colectie
        public DateTime AddedDate { get; set; }
    }
}
