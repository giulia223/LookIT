using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LookIT.Models
{
    public class ApplicationUser : IdentityUser
    {
       
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Numele trebuie să aibă între {2} și {1} caractere.")]
        public string? FullName { get; set; }

        [StringLength(500, ErrorMessage = "Descrierea nu poate depăși {1} caractere.")]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; } = "/images/profile/default_user.jpg";

        public bool Public { get; set; } = true;

        //un user poate avea mai multe postari
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        //un user poate posta mai multe comentarii
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        //un user poate modera mai multe grupuri
        public virtual ICollection<Group> ModeratedGroups { get; set; } = new List<Group>();

        //un user poate aprecia mai multe posatri
        public virtual ICollection<Like> LikedPosts { get; set; } = new List<Like>();

        //un user poate salva mai multe posatri
        public virtual ICollection<Save> SavedPosts { get; set; } = new List<Save>();

        //un user poate urmari mai multi useri
        public virtual ICollection<FollowRequest> SentFollowRequests { get; set; } = new List<FollowRequest>();

        //un user poate fi urmarit de mai multi useri
        public virtual ICollection<FollowRequest> ReceivedFollowRequests { get; set; } = new List<FollowRequest>();

        //un user poate fi membru in mai multe grupuri
        public virtual ICollection<GroupMember> Groups { get; set; } = new List<GroupMember>();

        //un user poate trimite mai multe mesaje in diverse grupuri
        public virtual ICollection <Message> SentMessages { get; set; } = new List<Message>();

        //variabila in care vom retine rolurile existente in baza de date 
        //pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}


