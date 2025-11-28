using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models.ViewModels;

namespace BeFit_Kaszkowiak.Controllers
{
    [Authorize]
    public class StatystykiController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StatystykiController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dane = await _context.TypyCwiczen
                .Select(t => new StatystykaTypuViewModel
                {
                    TypCwiczeniaId = t.Id,
                    NazwaTypu = t.Nazwa,
                    IloscCwiczen = _context.Cwiczenia.Count(c => c.TypCwiczeniaId == t.Id && c.Sesja.UserId == userId),
                    LacznyCzasSek = _context.Cwiczenia.Where(c => c.TypCwiczeniaId == t.Id && c.Sesja.UserId == userId).Sum(c => (int?)c.CzasTrwaniaSek) ?? 0,
                    SredniePowtorzenia = _context.Cwiczenia.Where(c => c.TypCwiczeniaId == t.Id && c.Sesja.UserId == userId).Any()
                        ? _context.Cwiczenia.Where(c => c.TypCwiczeniaId == t.Id && c.Sesja.UserId == userId).Average(c => (double)c.Powtorzenia)
                        : 0
                }).ToListAsync();

            return View(dane);
        }
    }
}
