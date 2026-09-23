using Microsoft.Playwright;

namespace PlaywrightTests.POMIA.Pages;

/// <summary>Картка товару.</summary>
public class ProductPage : BasePage
{
    public ProductPage(IPage page) : base(page)
    {
    }

    ILocator AddToCartButton => Page.Locator("[name='add_cart_product']");

    /// <summary>
    /// Тільки клік, без ручних очікувань: факт додавання перевіряє тест через
    /// Expect(Cart.Quantity), а це асерт із автоочікуванням.
    /// </summary>
    public async Task AddToCartAsync() => await AddToCartButton.ClickAsync();
}
