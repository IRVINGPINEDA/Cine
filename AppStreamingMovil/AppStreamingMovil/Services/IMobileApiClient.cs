using AppStreamingMovil.Models.Api;

namespace AppStreamingMovil.Services;

public interface IMobileApiClient
{
    string BaseUrl { get; set; }
    bool HasToken { get; }

    Task<(bool IsSuccess, string? ErrorMessage)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(
        string firstName,
        string lastNamePaternal,
        string? lastNameMaternal,
        string email,
        string password,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MobileMovie>> GetMoviesAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync();
}
