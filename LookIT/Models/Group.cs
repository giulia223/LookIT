using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required(ErrorMessage ="Numele grupului este obligatoriu")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Numele trebuie să aibă între {2} și {1} caractere.")]
        public string GroupName { get; set; }

        [Required(ErrorMessage ="Descrierea grupului este obligatorie")]
        [StringLength(500, ErrorMessage = "Descrierea nu poate depăși {1} caractere.")]
        public string Description { get; set; }

        public DateTime Date {  get; set; }

        public string? ModeratorId { get; set; }

        public virtual ApplicationUser? Moderator {get; set;}

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
