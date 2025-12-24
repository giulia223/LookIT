using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class PostCollection
    {
        //tabelul asociativ care face legatura intre Post si Collection
        //o postare poate fi salvata in mai multe colectii, iar o colectie
        //contine mai multe postari

        [Key]
        public int Id { get; set; }
        public int? PostId { get; set; }
        public int? CollectionId { get; set; }
        public virtual Post? Post { get; set; }
        public virtual Collection? Collection { get; set; }

        //data la care o postare a fost salvata intr-o anumita colectie
        public DateTime AddedDate { get; set; }
    }
}
