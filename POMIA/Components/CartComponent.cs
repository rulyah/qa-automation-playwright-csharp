using Microsoft.Playwright;
using PlaywrightTests.POMIA.Pages;

namespace PlaywrightTests.POMIA.Components;

/// <summary>Іконка кошика в хедері: показує кількість товарів і веде на оформлення замовлення.</summary>
public class CartComponent : BaseComponent
{
    public CartComponent(IPage page)
        : base(page, page.Locator("#cart"))
    {
    }

    /// <summary>
    /// Лічильник товарів. Віддаємо назовні як ILocator, а не як прочитаний рядок:
    /// тест зробить Expect(...).ToHaveTextAsync("1"), і Playwright сам дочекається оновлення.
    /// </summary>
    public ILocator Quantity => Root.Locator(".quantity");

    public async Task<CheckoutPage> OpenAsync()
    {
        await Root.ClickAsync();
        return new CheckoutPage(Page);
    }
}
