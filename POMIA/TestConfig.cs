namespace PlaywrightTests.POMIA;

/// <summary>
/// Єдине місце, де живуть налаштування оточення.
/// URL можна перевизначити змінною середовища LITECART_BASE_URL, не змінюючи код.
/// </summary>
public static class TestConfig
{
    const string DefaultBaseUrl = "http://localhost/litecart/";

    public static string BaseUrl { get; } =
        WithTrailingSlash(Environment.GetEnvironmentVariable("LITECART_BASE_URL") ?? DefaultBaseUrl);

    // трейлінг-слеш важливий: без нього відносний перехід "./" зріже останній сегмент шляху
    static string WithTrailingSlash(string url) => url.EndsWith('/') ? url : url + "/";
}
