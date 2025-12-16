using LookIT.Models;
using Microsoft.AspNetCore.Identity;
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
        public DbSet<Save> Saves { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //daca sterg un user, vor fi sterse si postarile asociate
            modelBuilder.Entity<Post>()
                .HasOne<ApplicationUser>(post => post.Author)
                .WithMany(user => user.Posts)
                .HasForeignKey(post => post.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            //am fost nevoita sa pun restrict pentru ca s-ar fi format ciclu 
            //daca un user are comentarii la postarile altor useri, nu il pot sterge
            //se face manual in controller : sterg comentariile si apoi userul
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

            //modelBuilder.Entity<FollowRequest>()
            //    .HasKey(followRequest => new { followRequest.FollowerId, followRequest.FollowingId});
            modelBuilder.Entity<FollowRequest>()
        .HasIndex(f => new { f.FollowerId, f.FollowingId })
        .IsUnique();
            //am fost nevoita sa pun restrict pentru ca s-ar fi format ciclu 
            //daca un user a dat follow altor useri, nu il pot sterge
            //se face manual in controller : sterg sentRequests si apoi userul
            modelBuilder.Entity<FollowRequest>()
                .HasOne<ApplicationUser>(followRequest => followRequest.Follower)
                .WithMany(follower => follower.SentFollowRequests)
                .HasForeignKey(followRequest => followRequest.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            //am fost nevoita sa pun restrict pentru ca s-ar fi format ciclu 
            //daca un user este urmarit de alti useri, nu il pot sterge
            //se face manual in controller : sterg receivedRequests si apoi userul
            modelBuilder.Entity<FollowRequest>()
                .HasOne<ApplicationUser>(followRequest => followRequest.Following)
                .WithMany(following => following.ReceivedFollowRequests)
                .HasForeignKey(followRequest => followRequest.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasKey(like => new { like.UserId, like.PostId });

            //am fost nevoita sa pun restrict pentru ca s-ar fi format ciclu 
            //daca un user are a dat like la postarile altor useri, nu il pot sterge
            //se face manual in controller : sterg like urile date si apoi userul
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

            modelBuilder.Entity<Save>()
                .HasKey(save => new { save.UserId, save.PostId });

            //am fost nevoita sa pun restrict pentru ca s-ar fi format ciclu 
            //daca un user a dat save la postarile altor useri, nu il pot sterge
            //se face manual in controller : sterg salvarile si apoi userul
            modelBuilder.Entity<Save>()
                .HasOne<ApplicationUser>(save => save.User)
                .WithMany(user => user.SavedPosts)
                .HasForeignKey(save => save.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //daca sterg o postare, vor fi sterse si salvarile asociate
            modelBuilder.Entity<Save>()
                .HasOne<Post>(save => save.Post)
                .WithMany(post => post.Saves)
                .HasForeignKey(save => save.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.Id)
                 .HasMaxLength(50);

            modelBuilder.Entity<IdentityUserToken<string>>()
                .Property(userToken => userToken.UserId)
                .HasMaxLength(50);

            modelBuilder.Entity<IdentityUserLogin<string>>()
                .Property(userLogin => userLogin.UserId)
                .HasMaxLength(50);

            modelBuilder.Entity<IdentityUserClaim<string>>()
                .Property(userClaims => userClaims.UserId)
                .HasMaxLength(50);

            modelBuilder.Entity<IdentityUserRole<string>>()
                .Property(userRole => userRole.UserId)
                .HasMaxLength(50);

            var userKeyNames = new[]
            {
                "UserId", 
                "ModeratorId", 
                "AuthorId",
                "FollowerId", 
                "FollowingId", 
                "MemberId"
            };

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var propertiesToUpdate = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(string) && userKeyNames.Contains(p.Name));

                foreach (var property in propertiesToUpdate)
                {
                    property.SetMaxLength(50);
                }
            }
        }
    }
}

