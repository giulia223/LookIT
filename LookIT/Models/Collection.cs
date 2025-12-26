using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Collection
    {
        [Key]
        public int CollectionId { get; set; }

        [Required(ErrorMessage ="Numele colecției este obligatoriu.")]
        [StringLength(30, ErrorMessage ="Numele colecției nu poate depăși {1} caractere.")]
        [Remote(action: "VerifyUniqueName", controller: "Collections")]
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
