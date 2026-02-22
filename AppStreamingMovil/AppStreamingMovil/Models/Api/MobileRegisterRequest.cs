namespace AppStreamingMovil.Models.Api;

public class MobileRegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastNamePaternal { get; set; } = string.Empty;
    public string? LastNameMaternal { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
