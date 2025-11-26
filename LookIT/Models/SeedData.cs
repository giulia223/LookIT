using LookIT.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LookIT.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext( serviceProvider
                .GetRequiredService <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un
               // rol
                // insemnand ca a fost rulat codul
                // De aceea facem return pentru a nu insera rolurile
                //inca o data7
            // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }
                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(

                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    Name = "Administrator",
                    NormalizedName = "Administrator".ToUpper()
                },

                new IdentityRole
                {
                    Id = "fd923264-a322-4bec-b112-54a05940b661", Name = "User", NormalizedName = "User".ToUpper() }
                );

                // o noua instanta pe care o vom utiliza pentru
                //crearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();

                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange( 
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    // primary key
                    UserName = "administrator@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMINISTRATOR@TEST.COM",
                    Email = "administrator@test.com",
                    NormalizedUserName = "ADMINISTRATOR@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,"Admin1!")
                },

                new ApplicationUser
                {
                    Id = "ca57146f-4211-4b21-8a2d-2c3e9611f50c",
                    // primary key
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                } );

                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {

                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },

                new IdentityUserRole<string>
                {
                    RoleId = "fd923264-a322-4bec-b112-54a05940b661",
                    UserId = "ca57146f-4211-4b21-8a2d-2c3e9611f50c"
                });
                context.SaveChanges();
            }
        }
    }
}
