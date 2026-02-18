using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models;

public class Movie
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public GenreType Genre { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ImagePath { get; set; }

    [Required]
    [Url]
    [StringLength(500)]
    public string TrailerUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
