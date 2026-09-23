using Microsoft.Playwright;

namespace PlaywrightTests.POMIA.Pages;

/// <summary>Сторінка кошика / оформлення замовлення.</summary>
public class CheckoutPage : BasePage
{
    public CheckoutPage(IPage page) : base(page)
    {
    }

    ILocator TermsAgreedCheckbox => Page.Locator("[name='terms_agreed']");
    ILocator ConfirmOrderButton => Page.Locator("[name='confirm_order']");

    /// <summary>Локатор результату відкритий назовні: перевірку робить тест, а не page object.</summary>
    public ILocator SuccessMessage => Page.Locator("#box-order-success .card-title");

    public async Task ConfirmOrderAsync()
    {
        // CheckAsync замість ClickAsync: ідемпотентний і сам перевіряє, що чекбокс справді став увімкненим
        await TermsAgreedCheckbox.CheckAsync();
        await ConfirmOrderButton.ClickAsync();
    }
}
