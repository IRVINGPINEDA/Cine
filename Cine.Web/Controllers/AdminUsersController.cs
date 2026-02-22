using Cine.Web.Models;
using Cine.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin/Users")]
public class AdminUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var items = new List<AdminUserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "-",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return View(items);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new AdminUserCreateViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "El correo ya esta registrado.");
            return View(model);
        }

        var generatedPassword = GenerateRandomPassword();
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FirstName = model.FirstName,
            LastNamePaternal = model.LastNamePaternal,
            LastNameMaternal = model.LastNameMaternal,
            IsActive = true,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, generatedPassword);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await _userManager.DeleteAsync(user);
            return View(model);
        }

        TempData["Success"] = "Usuario creado correctamente.";
        TempData["GeneratedPassword"] = generatedPassword;
        TempData["GeneratedPasswordEmail"] = user.Email;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Cliente";
        var model = new AdminUserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastNamePaternal = user.LastNamePaternal,
            LastNameMaternal = user.LastNameMaternal,
            Email = user.Email ?? string.Empty,
            Role = role,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminUserEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing is not null && existing.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "El correo ya esta registrado.");
            return View(model);
        }

        user.FirstName = model.FirstName;
        user.LastNamePaternal = model.LastNamePaternal;
        user.LastNameMaternal = model.LastNameMaternal;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.IsActive = model.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(model.Role))
        {
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }
        }

        TempData["Success"] = "Usuario actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleActive/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ResetPassword/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var generatedPassword = GenerateRandomPassword();
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, generatedPassword);
        if (!resetResult.Succeeded)
        {
            foreach (var error in resetResult.Errors)
            {
                TempData["Success"] = null;
                TempData["GeneratedPassword"] = null;
                TempData["GeneratedPasswordEmail"] = null;
                TempData["Error"] = error.Description;
            }

            return RedirectToAction(nameof(Index));
        }

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Contrasena temporal regenerada correctamente.";
        TempData["GeneratedPassword"] = generatedPassword;
        TempData["GeneratedPasswordEmail"] = user.Email;
        return RedirectToAction(nameof(Index));
    }

    private static string GenerateRandomPassword()
    {
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "23456789";
        const string symbols = "!@$?_";

        var random = new Random();
        var chars = new List<char>
        {
            lower[random.Next(lower.Length)],
            upper[random.Next(upper.Length)],
            digits[random.Next(digits.Length)],
            symbols[random.Next(symbols.Length)]
        };

        var all = lower + upper + digits + symbols;
        while (chars.Count < 12)
        {
            chars.Add(all[random.Next(all.Length)]);
        }

        return new string(chars.OrderBy(_ => random.Next()).ToArray());
    }
}

