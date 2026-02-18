using Cine.Web.Data;
using Cine.Web.Models;
using Cine.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin/Clients")]
public class AdminClientsController : Controller
{
    private readonly AppDbContext _dbContext;

    public AdminClientsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var clients = await _dbContext.Clients
            .OrderByDescending(c => c.RegisteredAt)
            .ToListAsync();

        return View(clients);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new ClientCreateViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _dbContext.Clients.AnyAsync(c => c.Email == model.Email);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Email), "El email ya existe.");
            return View(model);
        }

        _dbContext.Clients.Add(new Client
        {
            FullName = model.FullName,
            Email = model.Email,
            IsActive = model.IsActive,
            RegisteredAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Cliente registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _dbContext.Clients.FindAsync(id);
        if (client is null)
        {
            return NotFound();
        }

        var model = new ClientEditViewModel
        {
            Id = client.Id,
            FullName = client.FullName,
            Email = client.Email,
            IsActive = client.IsActive
        };

        return View(model);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = await _dbContext.Clients.FindAsync(id);
        if (client is null)
        {
            return NotFound();
        }

        var duplicatedEmail = await _dbContext.Clients
            .AnyAsync(c => c.Email == model.Email && c.Id != id);
        if (duplicatedEmail)
        {
            ModelState.AddModelError(nameof(model.Email), "El email ya existe.");
            return View(model);
        }

        client.FullName = model.FullName;
        client.Email = model.Email;
        client.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleActive/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var client = await _dbContext.Clients.FindAsync(id);
        if (client is null)
        {
            return NotFound();
        }

        client.IsActive = !client.IsActive;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
