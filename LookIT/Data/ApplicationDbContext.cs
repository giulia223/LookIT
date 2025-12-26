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
        public DbSet<Collection> Collections { get; set; }
        public DbSet<PostCollection> PostCollections { get; set; }

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

            //daca sterg o postare, vor fi sterse si comentariile asociate in cascada
            modelBuilder.Entity<Comment>()
                .HasOne<Post>(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            //am fost nevoita sa pun restrict pentru ca s-ar fi format ciclu 
            //daca un user este moderatorul cel putin unui grup, nu il pot sterge
            //se face manual in controller : sterg grupurile, iar mai apoi userul
            modelBuilder.Entity<Group>()
                .HasOne<ApplicationUser>(group => group.Moderator)
                .WithMany(moderator => moderator.ModeratedGroups)
                .HasForeignKey(group => group.ModeratorId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<FollowRequest>()
            //    .HasKey(followRequest => new { followRequest.FollowerId, followRequest.FollowingId});


            //constangere la nivel de baze de date: sa nu avem posibilitatea de a avea duplicate de tipul followerId, followngId
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
            //daca un user dat like la postarile altor useri, nu il pot sterge
            //se face manual in controller : sterg like urile date si apoi userul
            modelBuilder.Entity<Like>()
                .HasOne<ApplicationUser>(like => like.User)
                .WithMany(user => user.LikedPosts)
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //daca sterg o postare, vor fi sterse si like-urile asociate in cascada
            modelBuilder.Entity<Like>()
                .HasOne<Post>(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasKey(groupMember => new { groupMember.GroupId, groupMember.MemberId });

            //daca sterg un grup, vor fi sterse si inregistrarile din GroupMember corespunzatoare in cascada
            modelBuilder.Entity<GroupMember>()
                .HasOne<Group>(member => member.Group)
                .WithMany(groupMember => groupMember.Members)
                .HasForeignKey(member => member.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            //daca sterg un user, va fi sters si din grupurile din care facea parte ca si membru
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

            //daca sterg un grup, mesajele vor fi sterse automat in cascada
            modelBuilder.Entity<Message>()
                .HasOne<Group>(message => message.Group)
                .WithMany(group => group.Messages)
                .HasForeignKey(message => message.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            //constrangere la nivel de baze de date: sa nu avem aceesi postare de mai multe ori in aceeasi colectie
            modelBuilder.Entity<PostCollection>()
                .HasIndex(postCollection => new { postCollection.PostId, postCollection.CollectionId })
                .IsUnique();

            //cnstrangere la nivel de baze de date: sa nu avem 2 colectii cu acelasi nume ale aceluiasi utilizator
            modelBuilder.Entity<Collection>()
                .HasIndex(collection => new { collection.Name, collection.UserId })
                .IsUnique();


            //daca sterg o postare, se va sterge automat din colectiile utilizatorilor
            modelBuilder.Entity<PostCollection>()
                .HasOne(postCollection => postCollection.Post)
                .WithMany(post => post.PostCollections)
                .HasForeignKey(postCollection => postCollection.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            //dacs sterg o colectie, toate legaturile din PostCollections asociate acelei colectii
            //sunt si ele la randul lor sterse
            modelBuilder.Entity<PostCollection>()
                .HasOne(postCollection => postCollection.Collection)
                .WithMany(collection => collection.PostCollections)
                .HasForeignKey(postCollection => postCollection.CollectionId)
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

