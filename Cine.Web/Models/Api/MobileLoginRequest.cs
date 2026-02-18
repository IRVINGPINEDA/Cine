using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models.Api;

public class MobileLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
