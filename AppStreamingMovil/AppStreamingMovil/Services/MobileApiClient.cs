using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AppStreamingMovil.Configuration;
using AppStreamingMovil.Models.Api;

namespace AppStreamingMovil.Services;

public class MobileApiClient : IMobileApiClient
{
    private const string AuthTokenPreferenceKey = "mobile_auth_token";

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MobileApiClient(ApiSettings apiSettings)
    {
        _apiSettings = apiSettings;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public string BaseUrl
    {
        get => _apiSettings.BaseUrl;
        set => _apiSettings.BaseUrl = value;
    }

    public bool HasToken => !string.IsNullOrWhiteSpace(Preferences.Default.Get(AuthTokenPreferenceKey, string.Empty));

    public async Task<(bool IsSuccess, string? ErrorMessage)> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var loginRequest = new MobileLoginRequest
        {
            Email = email,
            Password = password
        };

        return await SendAuthRequestAsync("api/mobile/auth/login", loginRequest, cancellationToken);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(
        string firstName,
        string lastNamePaternal,
        string? lastNameMaternal,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var registerRequest = new MobileRegisterRequest
        {
            FirstName = firstName,
            LastNamePaternal = lastNamePaternal,
            LastNameMaternal = lastNameMaternal,
            Email = email,
            Password = password
        };

        return await SendAuthRequestAsync("api/mobile/auth/register", registerRequest, cancellationToken);
    }

    private async Task<(bool IsSuccess, string? ErrorMessage)> SendAuthRequestAsync(
        string endpoint,
        object requestBody,
        CancellationToken cancellationToken)
    {
        string? connectivityError = null;

        foreach (var baseUrl in _apiSettings.GetBaseUrlCandidates())
        {
            _apiSettings.BaseUrl = baseUrl;

            try
            {
                var uri = _apiSettings.BuildUri(endpoint);
                using var response = await _httpClient.PostAsJsonAsync(uri, requestBody, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return (false, await ReadErrorMessageAsync(response, cancellationToken));
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<MobileLoginResponse>(_jsonOptions, cancellationToken);
                if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.Token))
                {
                    return (false, "No se recibio token de autenticacion.");
                }

                Preferences.Default.Set(AuthTokenPreferenceKey, loginResponse.Token);
                return (true, null);
            }
            catch (HttpRequestException)
            {
                connectivityError = "No fue posible conectarse al API. Verifica que Docker/Caddy o la web local esten encendidos.";
            }
            catch (TaskCanceledException)
            {
                connectivityError = "La solicitud al API excedio el tiempo limite.";
            }
            catch (UriFormatException)
            {
                return (false, "La configuracion interna del API no tiene un formato valido.");
            }
            catch (JsonException)
            {
                connectivityError = "El API respondio con un formato inesperado.";
            }
            catch (Exception)
            {
                connectivityError = "Ocurrio un error inesperado al conectar con el API.";
            }
        }

        return (false, connectivityError ?? "No fue posible conectarse al API.");
    }

    public async Task<IReadOnlyList<MobileMovie>> GetMoviesAsync(CancellationToken cancellationToken = default)
    {
        var token = Preferences.Default.Get(AuthTokenPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("No hay sesion activa.");
        }
        Exception? lastConnectivityException = null;

        foreach (var baseUrl in _apiSettings.GetBaseUrlCandidates())
        {
            _apiSettings.BaseUrl = baseUrl;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _apiSettings.BuildUri("api/mobile/movies"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await LogoutAsync();
                    throw new UnauthorizedAccessException("La sesion expiro o no tiene permisos para consultar peliculas.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var message = await ReadErrorMessageAsync(response, cancellationToken);
                    throw new InvalidOperationException(message);
                }

                var movies = await response.Content.ReadFromJsonAsync<List<MobileMovie>>(_jsonOptions, cancellationToken) ?? [];

                foreach (var movie in movies)
                {
                    movie.ImageUrl = _apiSettings.ResolveImageUrl(string.IsNullOrWhiteSpace(movie.ImageUrl) ? movie.ImagePath : movie.ImageUrl);
                }

                return movies;
            }
            catch (HttpRequestException ex)
            {
                lastConnectivityException = ex;
            }
            catch (TaskCanceledException ex)
            {
                lastConnectivityException = ex;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El API respondio con un formato inesperado.", ex);
            }
        }

        if (lastConnectivityException is TaskCanceledException)
        {
            throw new InvalidOperationException("La solicitud al API excedio el tiempo limite.");
        }

        throw new InvalidOperationException("No fue posible conectarse al API.");
    }

    public Task LogoutAsync()
    {
        Preferences.Default.Remove(AuthTokenPreferenceKey);
        return Task.CompletedTask;
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var fallbackMessage = $"Error del API ({(int)response.StatusCode}).";

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return fallbackMessage;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);

            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                var message = messageElement.GetString();
                return string.IsNullOrWhiteSpace(message) ? fallbackMessage : message;
            }
        }
        catch (JsonException)
        {
            // Ignore parse errors and return fallback.
        }

        return fallbackMessage;
    }
}
