using Cine.Web.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Controllers.Api;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Cliente")]
[Route("api/mobile/movies")]
public class MobileMoviesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MobileMoviesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetMovies()
    {
        var movies = await _dbContext.Movies
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.Name,
                Genre = m.Genre.ToString(),
                m.Description,
                m.ImagePath,
                m.TrailerUrl
            })
            .ToListAsync();

        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMovie(int id)
    {
        var movie = await _dbContext.Movies
            .AsNoTracking()
            .Where(m => m.Id == id && m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.Name,
                Genre = m.Genre.ToString(),
                m.Description,
                m.ImagePath,
                m.TrailerUrl
            })
            .FirstOrDefaultAsync();

        if (movie is null)
        {
            return NotFound();
        }

        return Ok(movie);
    }
}
