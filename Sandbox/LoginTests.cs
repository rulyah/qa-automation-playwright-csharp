using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category("Login")]
public class LoginTests : PageTest
{
    private const string BaseUrl = "https://www.saucedemo.com/";
    private const string InventoryUrl = "https://www.saucedemo.com/inventory.html";

    private const string ValidUser = "standard_user";
    private const string ValidPassword = "secret_sauce";
    private const string InvalidPassword = "wrong_password";

    private const string CredentialsMismatchError =
        "Epic sadface: Username and password do not match any user in this service";

    private ILocator UsernameField => Page.GetByTestId("username");
    private ILocator PasswordField => Page.GetByTestId("password");
    private ILocator LoginButton => Page.GetByTestId("login-button");
    private ILocator ErrorMessage => Page.GetByTestId("error");
    private ILocator PageTitle => Page.GetByTestId("title");

    [SetUp]
    public async Task OpenLoginPage()
    {
        // saucedemo маркує елементи через data-test, а не data-testid
        Playwright.Selectors.SetTestIdAttribute("data-test");
        await Page.GotoAsync(BaseUrl);
    }

    [Test]
    public async Task Login_WithValidCredentials_OpensInventoryPage()
    {
        // Arrange
        await UsernameField.FillAsync(ValidUser);
        await PasswordField.FillAsync(ValidPassword);

        // Act
        await LoginButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(InventoryUrl);
        await Expect(PageTitle).ToHaveTextAsync("Products");
    }

    [Test]
    public async Task Login_WithInvalidPassword_ShowsCredentialsMismatchError()
    {
        // Arrange
        await UsernameField.FillAsync(ValidUser);
        await PasswordField.FillAsync(InvalidPassword);

        // Act
        await LoginButton.ClickAsync();

        // Assert
        await Expect(ErrorMessage).ToHaveTextAsync(CredentialsMismatchError);
    }
}
