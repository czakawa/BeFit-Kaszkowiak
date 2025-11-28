using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit_Kaszkowiak.Data;

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
            var since = DateTime.Today.AddDays(-28);

            var stats = await _context.Cwiczenia
                .Include(c=>c.TypCwiczenia)
                .Include(c=>c.Sesja)
                .Where(c=>c.Sesja!=null && c.Sesja.UserId==userId && c.Sesja.DataRozpoczecia>=since)
                .GroupBy(c=> new { c.TypCwiczeniaId, c.TypCwiczenia.Nazwa })
                .Select(g=> new {
                    TypId = g.Key.TypCwiczeniaId,
                    TypNazwa = g.Key.Nazwa,
                    Count = g.Count(),
                    TotalPowtorzen = g.Sum(x=> x.Powtorzenia),
                    AvgObciazenie = g.Average(x=> x.Obciazenie),
                    MaxObciazenie = g.Max(x=> x.Obciazenie)
                }).ToListAsync();

            return View(stats);
        }
    }
}
