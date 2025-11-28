using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.Extensions.Logging;

namespace BeFit_Kaszkowiak.Controllers
{
    [Authorize]
    public class CwiczeniaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CwiczeniaController> _logger;

        public CwiczeniaController(ApplicationDbContext context, ILogger<CwiczeniaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Cwiczenia
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = await _context.Cwiczenia
                        .Include(c => c.TypCwiczenia)
                        .Include(c => c.Sesja)
                        .Where(c => c.Sesja != null && c.Sesja.UserId == userId)
                        .OrderByDescending(c => c.Id)
                        .ToListAsync();
            return View(list);
        }

        // GET: Cwiczenia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia
                        .Include(c => c.TypCwiczenia)
                        .Include(c => c.Sesja)
                        .FirstOrDefaultAsync(c => c.Id == id && c.Sesja != null && c.Sesja.UserId == userId);

            if (cw == null) return NotFound();
            return View(cw);
        }

        // GET: Cwiczenia/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownsAsync();
            return View();
        }

        // POST: Cwiczenia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cwiczenie model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "U¿ytkownik niezalogowany.");
                await PopulateDropDownsAsync(model?.SesjaId, model?.TypCwiczeniaId);
                return View(model);
            }

            // walidacja
            if (model.TypCwiczeniaId == 0)
                ModelState.AddModelError(nameof(model.TypCwiczeniaId), "Musisz wybraæ typ æwiczenia.");
            if (model.SesjaId == 0)
                ModelState.AddModelError(nameof(model.SesjaId), "Musisz wybraæ sesjê.");

            // sprawdzenie przynale¿noœci sesji
            if (model.SesjaId != 0)
            {
                var sesja = await _context.Sesje.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == model.SesjaId && s.UserId == userId);
                if (sesja == null)
                    ModelState.AddModelError(nameof(model.SesjaId), "Wybrana sesja nie istnieje lub nie nale¿y do Ciebie.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model?.SesjaId, model?.TypCwiczeniaId);
                return View(model);
            }

            try
            {
                _context.Cwiczenia.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Æwiczenie zapisane.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d przy zapisie æwiczenia Create");
                ModelState.AddModelError(string.Empty, "B³¹d zapisu. Spróbuj ponownie.");
                await PopulateDropDownsAsync(model?.SesjaId, model?.TypCwiczeniaId);
                return View(model);
            }
        }

        // GET: Cwiczenia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia
                        .Include(c => c.Sesja)
                        .FirstOrDefaultAsync(c => c.Id == id && c.Sesja != null && c.Sesja.UserId == userId);

            if (cw == null) return NotFound();

            await PopulateDropDownsAsync(cw.SesjaId, cw.TypCwiczeniaId);
            return View(cw);
        }

        // POST: Cwiczenia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nazwa,Opis,CzasTrwaniaSek,Powtorzenia,TypCwiczeniaId, SesjaId")] Cwiczenie model)
        {
            if (id != model.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.Cwiczenia
                            .Include(c => c.Sesja)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == id && c.Sesja != null && c.Sesja.UserId == userId);
            if (existing == null) return NotFound();

            if (model.TypCwiczeniaId == 0)
                ModelState.AddModelError(nameof(model.TypCwiczeniaId), "Musisz wybraæ typ æwiczenia.");
            if (model.SesjaId == 0)
                ModelState.AddModelError(nameof(model.SesjaId), "Musisz wybraæ sesjê.");

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model.SesjaId, model.TypCwiczeniaId);
                return View(model);
            }

            // weryfikacja czy sesja nale¿y do usera
            var newSesja = await _context.Sesje.AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.SesjaId && s.UserId == userId);
            if (newSesja == null)
            {
                ModelState.AddModelError(nameof(model.SesjaId), "Wybrana sesja nie nale¿y do Ciebie.");
                await PopulateDropDownsAsync(model.SesjaId, model.TypCwiczeniaId);
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Zaktualizowano æwiczenie.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CwiczenieExists(model.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d przy zapisie Edit Cwiczenia Id={Id}", id);
                ModelState.AddModelError(string.Empty, "B³¹d podczas zapisu. Spróbuj ponownie.");
                await PopulateDropDownsAsync(model.SesjaId, model.TypCwiczeniaId);
                return View(model);
            }
        }

        // GET: Cwiczenia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia
                        .Include(c => c.TypCwiczenia)
                        .Include(c => c.Sesja)
                        .FirstOrDefaultAsync(c => c.Id == id && c.Sesja != null && c.Sesja.UserId == userId);

            if (cw == null) return NotFound();
            return View(cw);
        }

        // POST: Cwiczenia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia
                        .Include(c => c.Sesja)
                        .FirstOrDefaultAsync(c => c.Id == id && c.Sesja != null && c.Sesja.UserId == userId);

            if (cw == null) return NotFound();

            _context.Cwiczenia.Remove(cw);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Usuniêto æwiczenie.";
            return RedirectToAction(nameof(Index));
        }

        private bool CwiczenieExists(int id) =>
            _context.Cwiczenia.Any(e => e.Id == id);

        private async Task PopulateDropDownsAsync(int? selectedSesjaId = null, int? selectedTypId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var typy = await _context.TypyCwiczen
                .OrderBy(t => t.Nazwa)
                .ToListAsync();
            ViewData["TypCwiczeniaId"] = new SelectList(typy, "Id", "Nazwa", selectedTypId);

            var sesje = await _context.Sesje
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Data)
                .ToListAsync();
            ViewData["SesjaId"] = new SelectList(sesje, "Id", "Tytul", selectedSesjaId);
        }
    }
}
