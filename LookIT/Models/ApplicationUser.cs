using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LookIT.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Numele trebuie să aibă între {2} și {1} caractere.")]
        public string? FullName { get; set; }

        [StringLength(500, ErrorMessage = "Descrierea nu poate depăși {1} caractere.")]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public bool Public { get; set; } = true;

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Group> ModeratedGroups { get; set; } = new List<Group>();

        public virtual ICollection<Like> LikedPosts { get; set; } = new List<Like>();

        public virtual ICollection<Save> SavedPosts { get; set; } = new List<Save>();

        public virtual ICollection<FollowRequest> SentFollowRequests { get; set; } = new List<FollowRequest>();

        public virtual ICollection<FollowRequest> ReceivedFollowRequests { get; set; } = new List<FollowRequest>();

        public virtual ICollection<GroupMember> Groups { get; set; } = new List<GroupMember>();

        public virtual ICollection <Message> SentMessages { get; set; } = new List<Message>();

    }
}


