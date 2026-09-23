using Microsoft.Playwright;

namespace PlaywrightTests.POM;

public class TestBase
{
    const string Url = "http://localhost/litecart/";
    
    IPlaywright _playwright;
    IBrowser _browser;
    IBrowserContext _context;
    public IPage Page { get; private set; }
    
    [SetUp]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, 
            SlowMo = 700
        });
        
        _context = await _browser.NewContextAsync(new()
        {
            Locale = "en-US",
            ViewportSize = new() { Width = 1920, Height = 1080 }
        });
        
        //await _browser.NewContextAsync();
        Page = await _context.NewPageAsync();
        await Page.GotoAsync(Url);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}