using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models.ViewModels;

public class ClientEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    [Display(Name = "Email de contacto")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool IsActive { get; set; }
}
