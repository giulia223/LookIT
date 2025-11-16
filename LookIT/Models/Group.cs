using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        [Required(ErrorMessage ="Numele grupului este obligatoriu")]
        public string GroupName { get; set; }
        [Required(ErrorMessage ="Descrierea grupului este obligatorie")]
        public string Description { get; set; }
        public DateTime Date {  get; set; }
        [Required(ErrorMessage ="Moderaturul trebuie sa existe")]
        public string ModeratorId { get; set; }
        [Required(ErrorMessage = "Moderaturul trebuie sa existe")]
        public virtual ApplicationUser Moderator {get; set;}

        public virtual ICollection<GroupMember> Members { get; set; }
    }
}
