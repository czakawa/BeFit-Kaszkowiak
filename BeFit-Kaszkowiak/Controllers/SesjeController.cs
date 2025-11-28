using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.Extensions.Logging;

namespace BeFit_Kaszkowiak.Controllers
{
    [Authorize]
    public class SesjeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SesjeController> _logger;

        public SesjeController(ApplicationDbContext context, ILogger<SesjeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Sesje
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = await _context.Sesje
                        .Where(s => s.UserId == userId)
                        .OrderByDescending(s => s.Data)
                        .ToListAsync();
            return View(list);
        }

        // GET: Sesje/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sesja = await _context.Sesje
                        .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (sesja == null) return NotFound();
            return View(sesja);
        }

        // GET: Sesje/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sesje/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sesja model)
        {
            // przypisz UserId natychmiast, zanim sprawdzimy ModelState — 
            // dziêki temu walidacja i zapis bêd¹ widzieæ w³aœciciela
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // nie powinno siê zdarzyæ (Authorize), ale wy³apujemy
                ModelState.AddModelError(string.Empty, "U¿ytkownik nie jest zalogowany.");
                return View(model);
            }
            model.UserId = userId;

            // Serwerowa walidacja
            if (string.IsNullOrWhiteSpace(model.Tytul))
            {
                ModelState.AddModelError(nameof(model.Tytul), "Tytu³ jest wymagany.");
            }

            // Ustaw domyœln¹ datê jeœli u¿ytkownik jej nie poda
            if (model.Data == default(DateTime))
            {
                model.Data = DateTime.Today;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _context.Sesje.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sesja zapisana.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d zapisu sesji Create");
                ModelState.AddModelError(string.Empty, "Wyst¹pi³ b³¹d przy zapisie. Spróbuj ponownie.");
                return View(model);
            }
        }

        // GET: Sesje/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sesja = await _context.Sesje
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (sesja == null) return NotFound();
            return View(sesja);
        }

        // POST: Sesje/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sesja model)
        {
            if (id != model.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.Sesje.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (existing == null) return NotFound();

            // zachowaj UserId - zawsze w³aœcicielem pozostaje ten user
            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Zaktualizowano sesjê.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error podczas Edit Sesji Id={Id}", id);
                if (!await _context.Sesje.AnyAsync(e => e.Id == model.Id && e.UserId == userId))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d przy edycji sesji Id={Id}", id);
                ModelState.AddModelError(string.Empty, "B³¹d podczas zapisu zmian.");
                return View(model);
            }
        }

        // GET: Sesje/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sesja = await _context.Sesje.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (sesja == null) return NotFound();
            return View(sesja);
        }

        // POST: Sesje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sesja = await _context.Sesje.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (sesja == null) return NotFound();

            try
            {
                // usuñ powi¹zane æwiczenia, jeœli nie masz cascade delete
                var cwiczenia = _context.Cwiczenia.Where(c => c.SesjaId == sesja.Id);
                _context.Cwiczenia.RemoveRange(cwiczenia);

                _context.Sesje.Remove(sesja);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuniêto sesjê.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d przy usuwaniu sesji Id={Id}", id);
                ModelState.AddModelError(string.Empty, "B³¹d przy usuwaniu. Spróbuj ponownie.");
                return View(sesja);
            }
        }
    }
}
