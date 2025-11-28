using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var lista = await _context.Sesje
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.DataRozpoczecia) // jeśli w modelu pole ma inną nazwę, dopasuj
                .ToListAsync();

            return View(lista);
        }

        // GET: Sesje/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sesja = await _context.Sesje
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (sesja == null) return NotFound();

            return View(sesja);
        }

        // GET: Sesje/Create
        public IActionResult Create()
        {
            var model = new Sesja
            {
                DataRozpoczecia = DateTime.Now
            };
            return View(model);
        }

        // POST: Sesje/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tytul,Opis,DataRozpoczecia,DataZakonczenia")] Sesja model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Create: brak userId (użytkownik nie zalogowany?)");
                return Forbid();
            }

            // Jeśli klient nie przesłał daty - ustaw bieżącą
            if (model.DataRozpoczecia == default) model.DataRozpoczecia = DateTime.Now;

            // Walidacja dat
            if (model.DataZakonczenia.HasValue && model.DataZakonczenia.Value < model.DataRozpoczecia)
            {
                ModelState.AddModelError(nameof(model.DataZakonczenia), "Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.UserId = userId;
                _context.Sesje.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sesja została zapisana.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                var inner = dbEx.GetBaseException()?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Błąd DB przy zapisie Sesji: {Message}", inner);
                ModelState.AddModelError(string.Empty, "Błąd zapisu: " + inner);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Niezidentyfikowany błąd przy zapisie Sesji");
                ModelState.AddModelError(string.Empty, "Błąd zapisu: " + ex.Message);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tytul,Opis,DataRozpoczecia,DataZakonczenia")] Sesja model)
        {
            if (id != model.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();

            // Sprawdź, czy rekord należy do użytkownika
            var existing = await _context.Sesje
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (existing == null) return NotFound();

            if (model.DataRozpoczecia == default) model.DataRozpoczecia = existing.DataRozpoczecia;

            if (model.DataZakonczenia.HasValue && model.DataZakonczenia.Value < model.DataRozpoczecia)
            {
                ModelState.AddModelError(nameof(model.DataZakonczenia), "Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.");
            }

            if (!ModelState.IsValid) return View(model);

            try
            {
                model.UserId = userId; // zabezpieczenie - nie pozwalamy zmienić właściciela
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Zmieniono dane sesji.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException dce)
            {
                _logger.LogWarning(dce, "Concurrency error przy edycji sesji Id={Id}", id);
                if (!await SesjaExistsForUser(id, userId)) return NotFound();
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                var inner = dbEx.GetBaseException()?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Błąd DB przy edycji Sesji: {Message}", inner);
                ModelState.AddModelError(string.Empty, "Błąd zapisu: " + inner);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Niezidentyfikowany błąd przy edycji Sesji");
                ModelState.AddModelError(string.Empty, "Błąd zapisu: " + ex.Message);
                return View(model);
            }
        }

        // GET: Sesje/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sesja = await _context.Sesje
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

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
                _context.Sesje.Remove(sesja);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sesja została usunięta.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                var inner = dbEx.GetBaseException()?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Błąd DB przy usuwaniu Sesji: {Message}", inner);
                TempData["Error"] = "Błąd podczas usuwania: " + inner;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Niezidentyfikowany błąd przy usuwaniu Sesji");
                TempData["Error"] = "Błąd podczas usuwania: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> SesjaExistsForUser(int id, string userId)
        {
            return await _context.Sesje.AnyAsync(s => s.Id == id && s.UserId == userId);
        }
    }
}
