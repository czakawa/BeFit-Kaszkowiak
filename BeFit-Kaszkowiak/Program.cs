using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Pobranie connection stringa z appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity + role store
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Seed/Migrate w scope BEFORE app.Run() ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // 1) Apply pending migrations (jeœli s¹)
        var ctx = services.GetRequiredService<ApplicationDbContext>();
        ctx.Database.Migrate();

        // 2) Roles + admin
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        async Task EnsureRolesAndAdminAsync()
        {
            string[] roles = new[] { "Administrator", "User" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole(r));
            }

            var adminEmail = "admin@befit.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var res = await userManager.CreateAsync(admin, "P@ssword123!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
                else
                {
                    logger.LogWarning("Nie uda³o siê utworzyæ konta admin: {errors}", string.Join("; ", res.Errors.Select(e => e.Description)));
                }
            }
        }

        // 3) Seed typów i przyk³adowej sesji (idempotentnie)
        async Task EnsureSeedDataAsync()
        {
            var ctxLocal = services.GetRequiredService<ApplicationDbContext>();

            if (!ctxLocal.TypyCwiczen.Any())
            {
                ctxLocal.TypyCwiczen.AddRange(new[]
                {
                    new TypCwiczenia { Nazwa = "Kardio", Opis = "Bieganie, rower, orbitrek" },
                    new TypCwiczenia { Nazwa = "Si³a", Opis = "Æwiczenia si³owe" },
                    new TypCwiczenia { Nazwa = "Mobilnoœæ", Opis = "Stretching i mobilnoœæ" }
                });
                await ctxLocal.SaveChangesAsync();
            }

            // utworzymy sesjê dla admina (jeœli admin ju¿ istnieje)
            var adminUser = await userManager.FindByEmailAsync("admin@befit.local");
            if (adminUser != null)
            {
                if (!ctxLocal.Sesje.Any(s => s.UserId == adminUser.Id))
                {
                    ctxLocal.Sesje.Add(new Sesja { Tytul = "Sesja testowa", Data = DateTime.Today, Opis = "Sesja seed", UserId = adminUser.Id });
                    await ctxLocal.SaveChangesAsync();
                }
            }
        }

        // wykonaj seedy asynchronicznie
        await EnsureRolesAndAdminAsync();
        await EnsureSeedDataAsync();

        logger.LogInformation("Seed danych wykonany pomyœlnie.");
    }
    catch (Exception ex)
    {
        var loggerEx = services.GetRequiredService<ILogger<Program>>();
        loggerEx.LogError(ex, "B³¹d podczas migracji/seedowania bazy danych.");
        // nie przerywamy uruchamiania aplikacji — ale zalecamy naprawê b³êdu
    }
}

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
