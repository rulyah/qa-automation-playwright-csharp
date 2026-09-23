using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using PlaywrightTests.POMIA.Pages;

namespace PlaywrightTests.POMIA.Tests;

/// <summary>
/// Базовий тест. Успадкування від PageTest прибирає ручний life-cycle
/// (Playwright/Browser/Context/Page + Dispose) — Microsoft.Playwright.NUnit робить це сам,
/// дає готові Page, Context, Expect() і трейс/відео через .runsettings.
/// </summary>
public abstract class BaseTest : PageTest
{
    protected HomePage HomePage { get; private set; } = null!;

    /// <summary>Опції контексту для кожного тесту; BaseURL дозволяє відносні GotoAsync("./").</summary>
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = TestConfig.BaseUrl,
        Locale = "en-US",
        ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
    };

    [SetUp]
    public async Task OpenHomePage()
    {
        HomePage = new HomePage(Page);
        await HomePage.OpenAsync();
    }
}
