using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cine.Web.Data;
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
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public MobileAuthController(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
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

        return Ok(BuildLoginResponse(user));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] MobileRegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return Conflict(new { message = "Ya existe una cuenta con ese correo." });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastNamePaternal = request.LastNamePaternal.Trim(),
            LastNameMaternal = string.IsNullOrWhiteSpace(request.LastNameMaternal) ? null : request.LastNameMaternal.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var createMessage = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return BadRequest(new
            {
                message = string.IsNullOrWhiteSpace(createMessage)
                    ? "No se pudo registrar la cuenta."
                    : createMessage
            });
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Cliente");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var roleMessage = string.Join(" ", roleResult.Errors.Select(e => e.Description));
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = string.IsNullOrWhiteSpace(roleMessage)
                    ? "No se pudo asignar el rol de cliente."
                    : roleMessage
            });
        }

        _dbContext.Clients.Add(new Client
        {
            FullName = user.FullName,
            Email = email,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true
        });

        await _dbContext.SaveChangesAsync();

        return Ok(BuildLoginResponse(user));
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

    private MobileLoginResponse BuildLoginResponse(ApplicationUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var token = GenerateJwtToken(user, expiresAt);

        return new MobileLoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new MobileLoginUser
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = "Cliente"
            }
        };
    }
}
