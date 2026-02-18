using Cine.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Data;

public static class AppDbSeeder
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedMoviesAsync(context);
        await SeedClientsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Cliente" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await EnsureUserAsync(
            userManager,
            "admin@demo.com",
            "Admin123!",
            "Admin",
            "Admin",
            "Principal",
            null);

        await EnsureUserAsync(
            userManager,
            "cliente@demo.com",
            "Cliente123!",
            "Cliente",
            "Cliente",
            "Demo",
            null);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        string firstName,
        string lastNamePaternal,
        string? lastNameMaternal)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastNamePaternal = lastNamePaternal,
                LastNameMaternal = lastNameMaternal,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear usuario semilla {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedMoviesAsync(AppDbContext context)
    {
        if (await context.Movies.AnyAsync())
        {
            return;
        }

        var movies = new List<Movie>
        {
            new()
            {
                Name = "Horizonte de Acero",
                Genre = GenreType.Action,
                Description = "Un piloto retirado vuelve para evitar un conflicto global.",
                ImagePath = "/uploads/horizonte-acero.jpg",
                TrailerUrl = "https://www.youtube.com/watch?v=EXeTwQWrcwY",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Cartas al Tiempo",
                Genre = GenreType.Drama,
                Description = "Una historia de familia, memoria y segundas oportunidades.",
                ImagePath = "/uploads/cartas-tiempo.jpg",
                TrailerUrl = "https://www.youtube.com/watch?v=0pdqf4P9MB8",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Noche Binaria",
                Genre = GenreType.SciFi,
                Description = "En una ciudad conectada por IA, una falla revela un secreto oscuro.",
                ImagePath = "/uploads/noche-binaria.jpg",
                TrailerUrl = "https://www.youtube.com/watch?v=zSWdZVtXT7E",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedClientsAsync(AppDbContext context)
    {
        if (await context.Clients.AnyAsync())
        {
            return;
        }

        var clients = new List<Client>
        {
            new()
            {
                FullName = "Cliente Demo",
                Email = "cliente@demo.com",
                IsActive = true,
                RegisteredAt = DateTime.UtcNow
            },
            new()
            {
                FullName = "Maria Gonzalez",
                Email = "maria.gonzalez@demo.com",
                IsActive = true,
                RegisteredAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();
    }
}
