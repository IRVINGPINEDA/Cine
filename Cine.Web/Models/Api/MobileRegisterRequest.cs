using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models.Api;

public class MobileRegisterRequest
{
    [Required]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string LastNamePaternal { get; set; } = string.Empty;

    [StringLength(80)]
    public string? LastNameMaternal { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
