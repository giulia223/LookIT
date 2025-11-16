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
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //daca sterg un user, vor fi sterse si postarile asociate
            modelBuilder.Entity<Post>()
                .HasOne<ApplicationUser>(post => post.Author)
                .WithMany(user => user.Posts)
                .HasForeignKey(post => post.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            //daca sterg un user, NU vor fi sterse si comentariile asociate pt ca se face ciclu
            modelBuilder.Entity<Comment>()
                .HasOne<ApplicationUser>(comment =>comment.User)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //daca sterg o postare, vor fi sterse si comentariile asociate
            modelBuilder.Entity<Comment>()
                .HasOne<Post>(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            //daca sterg un user, NU vor fi sterse si grupurile pe care le modereaza
            modelBuilder.Entity<Group>()
                .HasOne<ApplicationUser>(group => group.Moderator)
                .WithMany(moderator => moderator.ModeratedGroups)
                .HasForeignKey(group => group.ModeratorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowRequest>()
                .HasKey(followRequest => new { followRequest.FollowerId, followRequest.FollowingId});

            //daca sterg un user, NU vor fi sterse si cererile de follow asociate
            modelBuilder.Entity<FollowRequest>()
                .HasOne<ApplicationUser>(followRequest => followRequest.Follower)
                .WithMany(follower => follower.SentFollowRequests)
                .HasForeignKey(followRequest => followRequest.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            //daca sterg un user, NU vor fi sterse si cererile de follow asociate
            modelBuilder.Entity<FollowRequest>()
                .HasOne<ApplicationUser>(followRequest => followRequest.Following)
                .WithMany(following => following.ReceivedFollowRequests)
                .HasForeignKey(followRequest => followRequest.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasKey(like => new { like.UserId, like.PostId });

            //daca sterg un user, NU vor fi sterse si like-urile asociate pt ca se face ciclu
            modelBuilder.Entity<Like>()
                .HasOne<ApplicationUser>(like => like.User)
                .WithMany(user => user.LikedPosts)
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //daca sterg o postare, vor fi sterse si like-urile asociate
            modelBuilder.Entity<Like>()
                .HasOne<Post>(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasKey(groupMember => new { groupMember.GroupId, groupMember.MemberId });

            //daca sterg un grup, vor fi sterse si inregistrarile din GroupMember corespunzatoare
            modelBuilder.Entity<GroupMember>()
                .HasOne<Group>(member => member.Group)
                .WithMany(groupMember => groupMember.Members)
                .HasForeignKey(member => member.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            //daca sterg un user, va fi sters si din grupurile din care facea parte
            modelBuilder.Entity<GroupMember>()
                .HasOne<ApplicationUser>(member => member.Member)
                .WithMany(groupMember => groupMember.Groups)
                .HasForeignKey(member => member.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            //daca sterg un user, mesajele vor ramane in baza de date, dar UserId va fi setat pe null
            //la nume va aparea "Utilizator sters"
            modelBuilder.Entity<Message>() 
                .HasOne<ApplicationUser>(message => message.User)
                .WithMany(user => user.SentMessages)
                .HasForeignKey(message => message.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            //daca sterg un grup, mesajele vor fi sterse automat
            modelBuilder.Entity<Message>()
                .HasOne<Group>(message => message.Group)
                .WithMany(group => group.Messages)
                .HasForeignKey(message => message.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

