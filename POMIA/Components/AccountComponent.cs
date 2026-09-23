using Microsoft.Playwright;
using PlaywrightTests.POMIA.TestData;

namespace PlaywrightTests.POMIA.Components;

/// <summary>Випадаюче меню акаунта в хедері разом із формою логіну всередині нього.</summary>
public class AccountComponent : BaseComponent
{
    public AccountComponent(IPage page)
        : base(page, page.Locator(".nav-item.account.dropdown"))
    {
    }

    // локатори полів прив'язані до name — стабільного атрибута форми,
    // а не до class="form-control", який є на кожному інпуті бутстрапа
    ILocator EmailField => Root.Locator("input[name='email']");
    ILocator PasswordField => Root.Locator("input[name='password']");
    ILocator LoginButton => Root.Locator("[name='login']");

    public async Task LoginAsync(TestUser user)
    {
        await Root.ClickAsync(); // клік по пункту меню розкриває дропдаун із формою
        await EmailField.FillAsync(user.Email);
        await PasswordField.FillAsync(user.Password);
        await LoginButton.ClickAsync();
    }
}
