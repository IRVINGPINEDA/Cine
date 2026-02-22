using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models.ViewModels;

public class ChangePasswordRequiredViewModel
{
    [Required(ErrorMessage = "La contrasena actual es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contrasena actual")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contrasena es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contrasena debe tener al menos 8 caracteres.")]
    [Display(Name = "Nueva contrasena")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contrasena.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "La confirmacion no coincide.")]
    [Display(Name = "Confirmar nueva contrasena")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
