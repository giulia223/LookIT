using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Collection
    {
        [Key]
        public int CollectionId { get; set; }

        //nu punem Required pentru ca atunci va esua la validarile din model in momentul adaugarii postarii la salvate
        //cheie externa (FK): o salvare este efectyata de un user

        [Required(ErrorMessage ="Numele colectiei este obligatoriu.")]
        public string Name { get; set; }

        //data la care a fost creata o anumita colectie
        public DateTime CreationDate { get; set; }

        //cheie externa (FK): o colectie este creata de catre un user
        public string? UserId { get; set; }

        //nu punem Required pentru ca atunci va esua la validarile din model in momentul adaugarii postarii la salvate
        //proprietatea de navigatie: o salvare este efectyata de un user
        public virtual ApplicationUser? User { get; set; }

        //relatia many-to-many intre Post si Collection
        public virtual ICollection<PostCollection> PostCollections { get; set; } = new List<PostCollection>();


    }
}
