using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models.ViewModels;

public class AdminUserCreateViewModel
{
    [Required]
    [StringLength(80)]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Apellido paterno")]
    public string LastNamePaternal { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Apellido materno")]
    public string? LastNameMaternal { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Correo electronico")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression("Admin|Cliente", ErrorMessage = "Rol invalido.")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = "Cliente";
}

