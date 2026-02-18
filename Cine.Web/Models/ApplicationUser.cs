using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Cine.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string LastNamePaternal { get; set; } = string.Empty;

    [StringLength(80)]
    public string? LastNameMaternal { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastNamePaternal} {LastNameMaternal}".Trim();
}
