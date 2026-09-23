using Microsoft.Playwright;
using PlaywrightTests.POMIA.Components;

namespace PlaywrightTests.POMIA.Pages;

/// <summary>
/// Базова сторінка. Хедер із меню акаунта та кошиком присутній на кожній сторінці магазину,
/// тому компоненти живуть тут — і доступні з будь-якого page object без дублювання.
/// </summary>
public abstract class BasePage
{
    protected IPage Page { get; }

    public AccountComponent Account { get; }
    public CartComponent Cart { get; }

    protected BasePage(IPage page)
    {
        Page = page;
        Account = new AccountComponent(page);
        Cart = new CartComponent(page);
    }
}
