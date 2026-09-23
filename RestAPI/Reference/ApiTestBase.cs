using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests.RestAPI.Reference;

/// <summary>
/// Спільна інфраструктура для API-тестів: контекст запитів, фабрика тестових
/// даних, автоматичне прибирання створеного і хелпери для перевірок.
///
/// Головний принцип: кожен тест сам створює все, що йому потрібно, і сам за
/// собою прибирає. Тести не залежать ні від вмісту db.json, ні один від одного,
/// тому їх можна запускати поодинці, разом і в будь-якому порядку.
/// </summary>
public abstract class ApiTestBase : PlaywrightTest
{
    protected const string BaseUrl = "http://localhost:3000";

    /// <summary>Id, якого гарантовано немає в базі — для негативних перевірок.</summary>
    protected const string MissingUserId = "no-such-user-00000";

    private static bool _serverChecked;

    /// <summary>URL ресурсів, створених поточним тестом, у порядку створення.</summary>
    private readonly List<string> _createdResources = [];

    private IAPIRequestContext? _api;

    protected IAPIRequestContext Api =>
        _api ?? throw new InvalidOperationException("Контекст запитів ще не створено.");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [SetUp]
    public async Task CreateApiContext()
    {
        // BaseURL задається один раз — далі в тестах лише відносні шляхи.
        // Це те, що дозволить перевести весь набір на інший стенд одним рядком.
        _api = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = BaseUrl
        });

        await EnsureServerIsRunningAsync();
    }

    [TearDown]
    public async Task CleanUpAndDisposeContext()
    {
        if (_api is null)
        {
            return;
        }

        // TearDown виконується і після провалених тестів — саме тому прибирання
        // живе тут, а не в кінці тестового методу: впав ассерт посередині,
        // а сміття все одно прибрано.
        foreach (string resourceUrl in Enumerable.Reverse(_createdResources))
        {
            // 404 тут очікуваний: тест міг видалити ресурс сам, або json-server
            // зніс його каскадно разом із батьківським записом. Тому статус не
            // перевіряємо — прибирання не має валити тест.
            await _api.DeleteAsync(resourceUrl);
        }

        _createdResources.Clear();
        await _api.DisposeAsync();
        _api = null;
    }

    /// <summary>
    /// Створює користувача через API і одразу ставить його в чергу на видалення.
    /// Дані унікальні, тому паралельні чи повторні прогони не конфліктують.
    /// </summary>
    protected async Task<User> CreateUserAsync(string role = "user", bool active = true)
    {
        string unique = Guid.NewGuid().ToString("N")[..8];

        UserPayload payload = new()
        {
            Name = $"probe-{unique}",
            Email = $"probe-{unique}@example.com",
            Role = role,
            Active = active
        };

        IAPIResponse response = await Api.PostAsync("/users", new() { DataObject = payload });
        await ShouldHaveStatusAsync(response, 201, "підготовка даних для тесту");

        User created = await ReadAsync<User>(response);
        TrackForCleanup($"/users/{created.Id}");

        return created;
    }

    /// <summary>Реєструє ресурс на видалення в TearDown.</summary>
    protected void TrackForCleanup(string resourceUrl) => _createdResources.Add(resourceUrl);

    /// <summary>Читає тіло відповіді у типізовану модель.</summary>
    protected static async Task<T> ReadAsync<T>(IAPIResponse response)
    {
        string body = await response.TextAsync();

        return JsonSerializer.Deserialize<T>(body, JsonOptions)
               ?? throw new InvalidOperationException($"Порожнє тіло відповіді від {response.Url}");
    }

    /// <summary>
    /// Перевіряє код відповіді і, якщо він не збігся, показує тіло у повідомленні.
    /// Голий Assert.That(status, Is.EqualTo(200)) каже лише «очікували 200, було 500»,
    /// і далі доводиться лізти в логи сервера. Тут причина видна одразу у звіті.
    /// </summary>
    protected static async Task ShouldHaveStatusAsync(IAPIResponse response, int expectedStatus, string? context = null)
    {
        string body = await response.TextAsync();
        string where = context is null ? string.Empty : $" [{context}]";

        Assert.That(response.Status, Is.EqualTo(expectedStatus),
            $"{response.Status} {response.StatusText} на {response.Url}{where}\nТіло відповіді: {Shorten(body)}");
    }

    private static string Shorten(string text) =>
        text.Length <= 500 ? text : $"{text[..500]}… (обрізано)";

    private async Task EnsureServerIsRunningAsync()
    {
        if (_serverChecked)
        {
            return;
        }

        try
        {
            await Api.GetAsync("/db");
            _serverChecked = true;
        }
        catch (PlaywrightException ex)
        {
            // Без цієї перевірки непіднятий сервер дає купу однакових падінь
            // із мережевою помилкою, і незрозуміло, що саме треба зробити.
            Assert.Fail($"Мок-сервер не відповідає на {BaseUrl}.\n" +
                        $"Запусти його: cd mock-api && npm start\n\n{ex.Message}");
        }
    }
}
