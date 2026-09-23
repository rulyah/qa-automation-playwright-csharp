using Microsoft.Playwright;

namespace PlaywrightTests.POM;

[TestFixture]
public class POMTest : TestBase
{
    const string UserLogin = "user1@email.com";
    const string UserPassword = "demo12";
    const string ProductTitle = "Purple Duck";
    const string SuccessLabel = "was completed successfully!";
    
    [Test]
    public async Task Test()
    {
        MainPage mainPage = new MainPage(Page);
        await mainPage.Login(UserLogin, UserPassword);
        await mainPage.AddProduct(ProductTitle);
        await mainPage.ConfirmPurchase();
        
        await Assertions.Expect(mainPage.SuccessLabel).ToContainTextAsync(SuccessLabel);
        /*Assert.That(await mainPage.SuccessLabel.InnerTextAsync(),
            Does.Contain(SuccessLabel));*/
    }
}