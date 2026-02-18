namespace Cine.Web.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "Cine.Web";
    public string Audience { get; set; } = "Cine.Mobile";
    public string SecretKey { get; set; } = "CHANGE_THIS_SUPER_SECRET_KEY_32CHARS_MIN";
    public int ExpiresMinutes { get; set; } = 120;
}
