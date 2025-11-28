using System;
using System.Linq;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;


var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Konfiguracja bazy danych
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Brak connection string 'DefaultConnection' w appsettings.json");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// -------------------------
// Identity + Roles
// -------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// -------------------------
// MVC
// -------------------------
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()               // pozwala na IViewLocalizer w widokach
    .AddDataAnnotationsLocalization();  // pozwala lokalizowaæ komunikaty DataAnnotations

var app = builder.Build();
var supportedCultures = new[] { new CultureInfo("pl-PL") };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pl-PL"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// -------------------------
// Middleware
// -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
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

// -------------------------
// Seed ról, admina i danych
// -------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // role i admin
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    string[] roles = new[] { "Administrator", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
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
    }

    // seed danych (typy æwiczeñ i przyk³adowa sesja dla admina)
    var ctx = services.GetRequiredService<ApplicationDbContext>();

    if (!ctx.TypyCwiczen.Any())
    {
        ctx.TypyCwiczen.AddRange(
            new TypCwiczenia { Nazwa = "Si³a", Opis = "Æwiczenia si³owe" },
            new TypCwiczenia { Nazwa = "Kardio", Opis = "Bieganie, rower" },
            new TypCwiczenia { Nazwa = "Mobilnoœæ", Opis = "Stretching i mobilnoœæ" }
        );
        ctx.SaveChanges();
    }

    // u¿ywam aliasu Data w modelu Sesja (je¿eli go masz) — jeœli nie, u¿yj DataRozpoczecia
    if (admin != null && !ctx.Sesje.Any(s => s.UserId == admin.Id))
    {
        ctx.Sesje.Add(new Sesja
        {
            Tytul = "Sesja testowa admin",
            Opis = "Sesja wygenerowana przy seedzie",
            // jeœli masz pole DataRozpoczecia w modelu, ustaw DataRozpoczecia (alias Data mo¿e byæ te¿ dostêpny)
            // tutaj ustawiam w³aœciwoœæ Data (alias) dla kompatybilnoœci
            DataRozpoczecia = DateTime.Now.AddDays(-1),
            UserId = admin.Id
        });
        ctx.SaveChanges();
    }
}

app.Run();
