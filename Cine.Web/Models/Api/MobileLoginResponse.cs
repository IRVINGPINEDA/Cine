namespace Cine.Web.Models.Api;

public class MobileLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public MobileLoginUser User { get; set; } = new();
}

public class MobileLoginUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
