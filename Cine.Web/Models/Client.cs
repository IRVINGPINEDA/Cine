using System.ComponentModel.DataAnnotations;

namespace Cine.Web.Models;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
