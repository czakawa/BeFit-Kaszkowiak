using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeFit_Kaszkowiak.Data;
using BeFit_Kaszkowiak.Models;
using Microsoft.AspNetCore.Authorization;

namespace BeFit_Kaszkowiak.Controllers
{
    [Authorize]
    public class CwiczeniaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CwiczeniaController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = await _context.Cwiczenia
                        .Include(c=>c.TypCwiczenia)
                        .Include(c=>c.Sesja)
                        .Where(c=>c.UserId==userId)
                        .OrderByDescending(c=>c.Id)
                        .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id==null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia.Include(c=>c.TypCwiczenia).Include(c=>c.Sesja)
                        .FirstOrDefaultAsync(c=>c.Id==id && c.UserId==userId);
            if (cw==null) return NotFound();
            return View(cw);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cwiczenie model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            model.UserId = userId;

            if (model.TypCwiczeniaId==0) ModelState.AddModelError(nameof(model.TypCwiczeniaId),"Musisz wybrać typ");
            if (model.SesjaId==0) ModelState.AddModelError(nameof(model.SesjaId),"Musisz wybrać sesję");

            // verify session ownership
            var sesja = await _context.Sesje.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==model.SesjaId && s.UserId==userId);
            if (sesja==null) ModelState.AddModelError(nameof(model.SesjaId),"Wybrana sesja nie należy do Ciebie.");

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model.SesjaId, model.TypCwiczeniaId);
                return View(model);
            }

            _context.Cwiczenia.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id==null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia.Include(c=>c.Sesja).AsNoTracking()
                        .FirstOrDefaultAsync(c=>c.Id==id && c.UserId==userId);
            if (cw==null) return NotFound();
            await PopulateDropDownsAsync(cw.SesjaId, cw.TypCwiczeniaId);
            return View(cw);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cwiczenie model)
        {
            if (id!=model.Id) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.Cwiczenia.AsNoTracking().FirstOrDefaultAsync(c=>c.Id==id && c.UserId==userId);
            if (existing==null) return NotFound();

            if (model.TypCwiczeniaId==0) ModelState.AddModelError(nameof(model.TypCwiczeniaId),"Musisz wybrać typ");
            if (model.SesjaId==0) ModelState.AddModelError(nameof(model.SesjaId),"Musisz wybrać sesję");

            var sesja = await _context.Sesje.AsNoTracking().FirstOrDefaultAsync(s=>s.Id==model.SesjaId && s.UserId==userId);
            if (sesja==null) ModelState.AddModelError(nameof(model.SesjaId),"Wybrana sesja nie należy do Ciebie.");

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model.SesjaId, model.TypCwiczeniaId);
                return View(model);
            }

            model.UserId = userId;
            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id==null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia.Include(c=>c.Sesja).Include(c=>c.TypCwiczenia)
                        .FirstOrDefaultAsync(c=>c.Id==id && c.UserId==userId);
            if (cw==null) return NotFound();
            return View(cw);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cw = await _context.Cwiczenia.FirstOrDefaultAsync(c=>c.Id==id && c.UserId==userId);
            if (cw==null) return NotFound();
            _context.Cwiczenia.Remove(cw);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDownsAsync(int? selectedSesjaId=null, int? selectedTypId=null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var typy = await _context.TypyCwiczen.OrderBy(t=>t.Nazwa).ToListAsync();
            ViewData["TypCwiczeniaId"] = new SelectList(typy,"Id","Nazwa",selectedTypId);

            var sesje = await _context.Sesje.Where(s=>s.UserId==userId).OrderByDescending(s=>s.DataRozpoczecia).ToListAsync();
            ViewData["SesjaId"] = new SelectList(sesje,"Id","Tytul",selectedSesjaId);
        }
    }
}
