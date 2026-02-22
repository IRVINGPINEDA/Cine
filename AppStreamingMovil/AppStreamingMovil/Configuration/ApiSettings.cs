namespace AppStreamingMovil.Configuration;

public class ApiSettings
{
    private const string BaseUrlPreferenceKey = "mobile_api_base_url";

    public string BaseUrl
    {
        get
        {
            var stored = Preferences.Default.Get(BaseUrlPreferenceKey, string.Empty);
            return string.IsNullOrWhiteSpace(stored) ? GetPlatformBaseUrls().First() : stored;
        }
        set => Preferences.Default.Set(BaseUrlPreferenceKey, NormalizeBaseUrl(value));
    }

    public Uri BuildUri(string relativePath)
    {
        var baseUri = new Uri(EnsureTrailingSlash(BaseUrl), UriKind.Absolute);
        return new Uri(baseUri, relativePath.TrimStart('/'));
    }

    public string ResolveImageUrl(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var baseUri = new Uri(EnsureTrailingSlash(BaseUrl), UriKind.Absolute);
        return new Uri(baseUri, imagePath.TrimStart('/')).ToString();
    }

    public string NormalizeBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return GetPlatformBaseUrls().First();
        }

        var candidate = url.Trim();

        if (!candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            candidate = $"http://{candidate}";
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var normalizedUri))
        {
            return GetPlatformBaseUrls().First();
        }

        var builder = new UriBuilder(normalizedUri)
        {
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri.ToString().TrimEnd('/');
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith('/') ? url : $"{url}/";
    }

    public IReadOnlyList<string> GetBaseUrlCandidates()
    {
        var candidates = new List<string>();

        void AddCandidate(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            var normalized = NormalizeBaseUrl(url);
            if (!candidates.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(normalized);
            }
        }

        foreach (var candidate in GetPlatformBaseUrls())
        {
            AddCandidate(candidate);
        }

        // Se agrega al final para no priorizar una URL vieja guardada en desarrollo.
        AddCandidate(Preferences.Default.Get(BaseUrlPreferenceKey, string.Empty));

        return candidates;
    }

    private static IReadOnlyList<string> GetPlatformBaseUrls()
    {
#if ANDROID
        return
        [
            "https://caleiro.online", // produccion
            "https://www.caleiro.online" // produccion (www)
        ];
#else
        return
        [
            "https://caleiro.online", // produccion
            "https://www.caleiro.online" // produccion (www)
        ];
#endif
    }
}
