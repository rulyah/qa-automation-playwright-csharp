using Microsoft.Playwright;

namespace PlaywrightTests.POMIA.Pages;

/// <summary>Головна сторінка магазину з блоком "Latest products".</summary>
public class HomePage : BasePage
{
    public HomePage(IPage page) : base(page)
    {
    }

    ILocator LatestProducts => Page.Locator("#box-latest-products");

    /// <summary>"./" — відносний перехід від BaseURL, заданого в опціях контексту.</summary>
    public async Task OpenAsync() => await Page.GotoAsync("./");

    /// <summary>
    /// Дія, що змінює сторінку, повертає наступний page object —
    /// так тест читається як сценарій і не може випадково "залишитись" на старій сторінці.
    /// </summary>
    public async Task<ProductPage> OpenProductAsync(string productTitle)
    {
        await LatestProducts.GetByTitle(productTitle).ClickAsync();
        return new ProductPage(Page);
    }
}
