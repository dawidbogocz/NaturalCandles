using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NaturalCandles.Models;
using NaturalCandles.Models.ViewModels;
using NaturalCandles.Utility;

namespace NaturalCandles.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRoleVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserRoleVM
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    CurrentRole = roles.FirstOrDefault() ?? SD.Role_Customer
                });
            }

            return View(model);
        }

        public async Task<IActionResult> EditRole(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var vm = new EditUserRoleVM
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CurrentRole = currentRoles.FirstOrDefault(),
                RoleList = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Text = r.Name!,
                        Value = r.Name!
                    })
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(EditUserRoleVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.RoleList = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Text = r.Name!,
                        Value = r.Name!
                    });

                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains(SD.Role_Admin) && vm.NewRole != SD.Role_Admin)
            {
                var allAdmins = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
                if (allAdmins.Count <= 1)
                {
                    ModelState.AddModelError("", "You cannot remove the last admin role.");
                    vm.RoleList = _roleManager.Roles.Select(r => new SelectListItem
                    {
                        Text = r.Name!,
                        Value = r.Name!
                    });
                    return View(vm);
                }
            }

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove current role.");
                    vm.RoleList = _roleManager.Roles.Select(r => new SelectListItem
                    {
                        Text = r.Name!,
                        Value = r.Name!
                    });
                    return View(vm);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, vm.NewRole);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to assign new role.");
                vm.RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name!,
                    Value = r.Name!
                });
                return View(vm);
            }

            TempData["success"] = "User role updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}