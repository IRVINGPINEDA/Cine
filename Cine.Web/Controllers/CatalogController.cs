using Cine.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Controllers;

[Authorize(Roles = "Cliente,Admin")]
[Route("Catalog")]
public class CatalogController : Controller
{
    private readonly AppDbContext _dbContext;

    public CatalogController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var movies = await _dbContext.Movies
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return View(movies);
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var movie = await _dbContext.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

        if (movie is null)
        {
            return NotFound();
        }

        return View(movie);
    }
}
