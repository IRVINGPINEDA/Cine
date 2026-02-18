using System.ComponentModel.DataAnnotations;
using Cine.Web.Models;

namespace Cine.Web.Models.ViewModels;

public class MovieEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Genero")]
    public GenreType Genre { get; set; }

    [Required]
    [StringLength(2000)]
    [Display(Name = "Descripcion")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Imagen")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }

    [Required]
    [Url]
    [StringLength(500)]
    [Display(Name = "URL del trailer")]
    public string TrailerUrl { get; set; } = string.Empty;

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;
}

