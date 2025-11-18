using Microsoft.AspNetCore.Identity;

namespace LookIT.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public string? Description { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public bool Public { get; set; } = true;

        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Group> ModeratedGroups { get; set; }

        public virtual ICollection<Like> LikedPosts { get; set; }

        public virtual ICollection<Save> SavedPosts { get; set; }

        public virtual ICollection<FollowRequest> SentFollowRequests { get; set; }
        public virtual ICollection<FollowRequest> ReceivedFollowRequests { get; set; }

        public virtual ICollection<GroupMember> Groups { get; set; }

        public virtual ICollection <Message> SentMessages { get; set; }

    }
}


