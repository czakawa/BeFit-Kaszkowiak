//using beFit.Models;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeFit_Kaszkowiak.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TypCwiczenia> TypyCwiczen { get; set; }
        public DbSet<Sesja> Sesje { get; set; }
        public DbSet<Cwiczenie> Cwiczenia { get; set; }
    }
}
