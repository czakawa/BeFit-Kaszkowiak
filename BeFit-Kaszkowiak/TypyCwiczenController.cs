using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Authorization;

namespace BeFit_Kaszkowiak.Controllers
{
    public class TypyCwiczenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TypyCwiczenController(ApplicationDbContext context) => _context = context;

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.TypyCwiczen.OrderBy(t => t.Nazwa).ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var typ = await _context.TypyCwiczen.FirstOrDefaultAsync(m => m.Id == id);
            if (typ == null) return NotFound();
            return View(typ);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(TypCwiczenia typ)
        {
            if (!ModelState.IsValid) return View(typ);
            _context.Add(typ);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var typ = await _context.TypyCwiczen.FindAsync(id);
            if (typ == null) return NotFound();
            return View(typ);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, TypCwiczenia typ)
        {
            if (id != typ.Id) return NotFound();
            if (!ModelState.IsValid) return View(typ);
            try
            {
                _context.Update(typ);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException) { if (!_context.TypyCwiczen.Any(e => e.Id == typ.Id)) return NotFound(); throw; }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var typ = await _context.TypyCwiczen.FindAsync(id);
            if (typ == null) return NotFound();
            return View(typ);
        }

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
