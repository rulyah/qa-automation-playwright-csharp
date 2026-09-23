using PlaywrightTests.POMIA.TestData;

namespace PlaywrightTests.POMIA.Tests;

[TestFixture]
[Category("Checkout")]
public class CheckoutTests : BaseTest
{
    const string ProductTitle = "Purple Duck";
    const string SuccessText = "was completed successfully!";

    [Test]
    public async Task Customer_ConfirmsOrder_SeesSuccessMessage()
    {
        // Arrange
        await HomePage.Account.LoginAsync(TestUser.Default);

        // Act
        var productPage = await HomePage.OpenProductAsync(ProductTitle);
        await productPage.AddToCartAsync();

        // точка синхронізації: асерт із автоочікуванням замість WaitForAsync по ":not(:empty)"
        await Expect(productPage.Cart.Quantity).ToHaveTextAsync("1");

        var checkoutPage = await productPage.Cart.OpenAsync();
        await checkoutPage.ConfirmOrderAsync();

        // Assert
        await Expect(checkoutPage.SuccessMessage).ToContainTextAsync(SuccessText);
    }
}
