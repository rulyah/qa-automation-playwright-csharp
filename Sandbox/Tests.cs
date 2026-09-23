using Microsoft.Playwright;

namespace PlaywrightTests;

[TestFixture]
public class Tests
{
    IPlaywright _playwright;
    IBrowser _browser;
    IBrowserContext _context;
    IPage _page;
    
    [SetUp]
    public async Task SetUp()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,   // показати вікно
            SlowMo = 700        // пауза в мс між діями, щоб встигати дивитись
        });
        
        //await _playwright.Chromium.LaunchAsync();
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
    
    [Test]
    public async Task ShowErrorMassageWenLoginOrPasswordIncorrect()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        await SendUserData("standard_user", "secret_sauc");
        
        ILocator error = _page.Locator("[data-test='error']"); // css - *[data-test='error'], xPath - //*[@data-test = 'error']
        string errorMassage = "Epic sadface: Username and password do not match any user in this service";
        string errorText = await error.InnerTextAsync();
        
        Assert.That(errorText, Is.EqualTo(errorMassage));
    }

    [Test]
    public async Task RedirectToShopAfterLoginAndPasswordCorrect()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        await SendUserData("standard_user", "secret_sauce");
        
        ILocator pageHeaderLocator = _page.Locator(".app_logo"); // css - ".app_logo", xPath - //*[contains(@class, 'app_logo')]
        string pageHeader = await pageHeaderLocator.InnerTextAsync();
        string targetPageHeader = "Swag Labs";
        
        Assert.That(pageHeader, Is.EqualTo(targetPageHeader));
    }

    [Test]
    public async Task CreatingMultiWindowAndUsingDelay()
    {
        _context.SetDefaultTimeout(5000);
        await _page.GotoAsync("https://www.saucedemo.com/");
        
        IPage newPage = await _context.NewPageAsync();
        await newPage.GotoAsync("https://www.google.com/?hl=uk");
        
        ILocator inputField = newPage.Locator("textarea[name='q']"); // css - "textarea[name='q']", xPath - "//textarea[@name='q']"
        await inputField.FillAsync("кавун");
        await newPage.Keyboard.PressAsync("Enter");
        await newPage.CloseAsync();
        
        await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)"); // JS
        // await locator.ScrollIntoViewIfNeededAsync() автоскрол до елемента якщо потрібно

        await Task.Delay(10000);
    }
    
    async Task SendUserData(string login, string password)
    {
        ILocator loginField = _page.Locator("#user-name"); // css - "#user-name", xPath - //*[@id='user-name']
        await loginField.FillAsync(login);

        ILocator passwordField = _page.Locator("#password"); // css - "#password", xPath - //*[@id='password']
        await passwordField.FillAsync(password);
        
        ILocator loginButton = _page.Locator("#login-button"); // css - "#login-button", xPath - //*[@id='login-button']
        await loginButton.ClickAsync();
    }
    
    /*
    Таблиця відповідностей

  // By.Id("elementId")
  _page.Locator("#elementId");

  // By.ClassName("btn")
  _page.Locator(".btn");

  // By.CssSelector("form.login input[type=submit]")
  _page.Locator("form.login input[type=submit]");

  // By.Name("email")
  _page.Locator("[name='email']");

  // By.TagName("h1")
  _page.Locator("h1");

  // By.XPath("//div[@id='main']")
  _page.Locator("//div[@id='main']");     // працює, але без потреби краще не
  використовувати

  // By.LinkText("Увійти")
  _page.GetByRole(AriaRole.Link, new() { Name = "Увійти", Exact = true });
     */
}