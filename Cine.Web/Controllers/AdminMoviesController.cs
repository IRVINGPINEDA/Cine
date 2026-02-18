using Cine.Web.Data;
using Cine.Web.Models;
using Cine.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin/Movies")]
public class AdminMoviesController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public AdminMoviesController(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var movies = await _dbContext.Movies
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return View(movies);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new MovieCreateViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var movie = new Movie
        {
            Name = model.Name,
            Genre = model.Genre,
            Description = model.Description,
            TrailerUrl = model.TrailerUrl,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        if (model.ImageFile is not null && model.ImageFile.Length > 0)
        {
            movie.ImagePath = await SaveImageAsync(model.ImageFile);
        }

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        TempData["Success"] = "Pelicula registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        var model = new MovieEditViewModel
        {
            Id = movie.Id,
            Name = movie.Name,
            Genre = movie.Genre,
            Description = movie.Description,
            ExistingImagePath = movie.ImagePath,
            TrailerUrl = movie.TrailerUrl,
            IsActive = movie.IsActive
        };

        return View(model);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MovieEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        movie.Name = model.Name;
        movie.Genre = model.Genre;
        movie.Description = model.Description;
        movie.TrailerUrl = model.TrailerUrl;
        movie.IsActive = model.IsActive;

        if (model.ImageFile is not null && model.ImageFile.Length > 0)
        {
            movie.ImagePath = await SaveImageAsync(model.ImageFile);
        }

        await _dbContext.SaveChangesAsync();
        TempData["Success"] = "Pelicula actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Activate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        movie.IsActive = true;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Inactivate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inactivate(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        movie.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var uploadsRootFolder = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsRootFolder);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRootFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }
}

