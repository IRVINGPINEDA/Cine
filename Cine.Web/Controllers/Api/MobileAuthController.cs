using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cine.Web.Models;
using Cine.Web.Models.Api;
using Cine.Web.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cine.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/mobile/auth")]
public class MobileAuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwtOptions;

    public MobileAuthController(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] MobileLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            return Unauthorized(new { message = "Credenciales invalidas." });
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            return Unauthorized(new { message = "Credenciales invalidas." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Admins no pueden iniciar sesi\u00F3n en m\u00F3vil"
            });
        }

        if (!roles.Contains("Cliente"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Solo clientes pueden iniciar sesi\u00F3n en m\u00F3vil."
            });
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var token = GenerateJwtToken(user, expiresAt);

        return Ok(new MobileLoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new MobileLoginUser
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = "Cliente"
            }
        });
    }

    private string GenerateJwtToken(ApplicationUser user, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, "Cliente"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

