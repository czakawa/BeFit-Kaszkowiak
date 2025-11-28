using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeFit_Kaszkowiak.Controllers
{
    [Authorize] // domyślnie wymagamy autoryzacji; szczegółowe metody mogą używać AllowAnonymous/Authorize(Roles="...")
    public class TypyCwiczenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TypyCwiczenController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _context.TypyCwiczen.OrderBy(t => t.Nazwa).ToListAsync();
            return View(list);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var typ = await _context.TypyCwiczen.FirstOrDefaultAsync(m => m.Id == id.Value);
            if (typ == null) return NotFound();

            return View(typ);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Nazwa,Opis")] TypCwiczenia typ)
        {
            if (!ModelState.IsValid) return View(typ);

            _context.Add(typ);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var typ = await _context.TypyCwiczen.FindAsync(id.Value);
            if (typ == null) return NotFound();

            return View(typ);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nazwa,Opis")] TypCwiczenia typ)
        {
            if (id != typ.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(typ);
            }

            try
            {
                _context.Update(typ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TypyCwiczen.AnyAsync(e => e.Id == typ.Id))
                    return NotFound();
                throw;
            }
        }

        // GET: Delete (confirmation)
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var typ = await _context.TypyCwiczen.FirstOrDefaultAsync(m => m.Id == id.Value);
            if (typ == null) return NotFound();

            return View(typ);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var typ = await _context.TypyCwiczen.FindAsync(id);
            if (typ != null)
            {
                _context.TypyCwiczen.Remove(typ);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
