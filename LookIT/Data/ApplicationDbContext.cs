using LookIT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LookIT.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<FollowRequest> FollowRequests { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasOne<ApplicationUser>(post => post.Author)
                .WithMany(user => user.Posts)
                .HasForeignKey(post => post.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne<ApplicationUser>(comment =>comment.User)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne<Post>(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Group>()
                .HasOne<ApplicationUser>(group => group.Moderator)
                .WithMany(moderator => moderator.ModeratedGroups)
                .HasForeignKey(group => group.ModeratorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowRequest>()
                .HasKey(followRequest => new { followRequest.FollowerId, followRequest.FollowingId});

            modelBuilder.Entity<FollowRequest>()
                .HasOne<ApplicationUser>(followRequest => followRequest.Follower)
                .WithMany(follower => follower.SentFollowRequests)
                .HasForeignKey(followRequest => followRequest.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowRequest>()
                .HasOne<ApplicationUser>(followRequest => followRequest.Following)
                .WithMany(following => following.ReceivedFollowRequests)
                .HasForeignKey(followRequest => followRequest.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasKey(like => new { like.UserId, like.PostId });

            modelBuilder.Entity<Like>()
                .HasOne<ApplicationUser>(like => like.User)
                .WithMany(user => user.LikedPosts)
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne<Post>(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasKey(groupMember => new { groupMember.GroupId, groupMember.MemberId });

            modelBuilder.Entity<GroupMember>()
                .HasOne<Group>(member => member.Group)
                .WithMany(groupMember => groupMember.Members)
                .HasForeignKey(member => member.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne<ApplicationUser>(member => member.Member)
                .WithMany(groupMember => groupMember.Groups)
                .HasForeignKey(member => member.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

