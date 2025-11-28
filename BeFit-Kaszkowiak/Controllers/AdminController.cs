using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BeFit_Kaszkowiak.Models;

namespace BeFit_Kaszkowiak.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserWithRolesViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new UserWithRolesViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? u.Email,
                    Email = u.Email ?? "",
                    Roles = roles
                });
            }
            return View(model);
        }

        // GET: Admin/ManageRoles/{userId}
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var vm = new ManageRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? user.Email ?? user.Id,
                Roles = allRoles.Select(r => new RoleCheckbox { RoleName = r, Selected = userRoles.Contains(r) }).ToList()
            };

            return View(vm);
        }

        // POST: Admin/ManageRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageRolesViewModel model)
        {
            if (model == null) return BadRequest();

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName).ToArray();
            var currentRoles = await _userManager.GetRolesAsync(user);

            // add missing
            var toAdd = selectedRoles.Except(currentRoles).ToArray();
            if (toAdd.Any())
            {
                var res = await _userManager.AddToRolesAsync(user, toAdd);
                if (!res.Succeeded)
                {
                    ModelState.AddModelError("", "Nie uda³o siê dodaæ ról: " + string.Join(", ", res.Errors.Select(e => e.Description)));
                    return View(model);
                }
            }

            // remove unchecked
            var toRemove = currentRoles.Except(selectedRoles).ToArray();
            if (toRemove.Any())
            {
                var res = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!res.Succeeded)
                {
                    ModelState.AddModelError("", "Nie uda³o siê usun¹æ ról: " + string.Join(", ", res.Errors.Select(e => e.Description)));
                    return View(model);
                }
            }

            TempData["Success"] = "Zaktualizowano role u¿ytkownika.";
            return RedirectToAction(nameof(Index));
        }

        // Optional: create new role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Nazwa roli nie mo¿e byæ pusta.";
                return RedirectToAction(nameof(Index));
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = "Taka rola ju¿ istnieje.";
                return RedirectToAction(nameof(Index));
            }

            var res = await _roleManager.CreateAsync(new IdentityRole(roleName));
            TempData["Success"] = res.Succeeded ? "Utworzono rolê." : "B³¹d tworzenia roli.";
            return RedirectToAction(nameof(Index));
        }

        // Optional: delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var res = await _userManager.DeleteAsync(user);
            TempData["Success"] = res.Succeeded ? "U¿ytkownik usuniêty." : "B³¹d usuwania.";
            return RedirectToAction(nameof(Index));
        }
    }
}
