using LookIT.Data;
using LookIT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
         policy => policy.RequireRole("Administrator"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // Preluarea serviciilor
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. CREEAZA ROLUL "Administrator" (daca nu exista)
        string admsRoleName = "Administrator";
        if (await roleManager.FindByNameAsync(admsRoleName) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(admsRoleName));
            //System.Diagnostics.Debug.WriteLine("Rolul 'Administrator' a fost creat.");
        }

        // 2. GASESTE CONTUL TAU
        
        string initialAdmsEmail = "riclea.amalia@gmail.com";
        var admsUser = await userManager.FindByEmailAsync(initialAdmsEmail);

        if (admsUser != null)
        {
            // 3. ATRIBUIE ROLUL (dacã nu il are deja)
            if (!await userManager.IsInRoleAsync(admsUser, admsRoleName))
            {
                await userManager.AddToRoleAsync(admsUser, admsRoleName);
               // System.Diagnostics.Debug.WriteLine($"Utilizatorul {initialAdmsEmail} a fost promovat la Administrator.");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ATENTIE: Utilizatorul cu email-ul {initialAdmsEmail} nu a fost gasit. Nu s-a putut atribui rolul de Administrator.");
        }
    }
    catch (Exception ex)
    {
        // In caz de eroare, logheaza problema
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "A aparut o eroare la crearea rolurilor/utilizatorilor initiali.");
    }
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
