using Microsoft.Playwright;

namespace PlaywrightTests.POM;

public class MainPage
{
     ILocator _accountDropdown => _page.Locator(".nav-item.account.dropdown");
     ILocator _usernameField => _page.Locator("input[type='email'][name='email']");
     ILocator _passwordField => _page.Locator("input[name='password']");
     ILocator _submitButton => _page.Locator("[name='login']");
     ILocator _latestProducts => _page.Locator("#box-latest-products");
     ILocator _addProductButton => _page.Locator("[name='add_cart_product']");
     ILocator _basketButton => _page.Locator("#cart");
     ILocator _termsAgreedCheckbox => _page.Locator("[name='terms_agreed']");
     ILocator _confirmOrderButton => _page.Locator("[name='confirm_order']");
     ILocator _cartQuantity => _page.Locator("#cart .quantity:not(:empty)");
     public ILocator SuccessLabel => _page.Locator("#box-order-success .card-title");
     
     IPage _page;
     
     public MainPage(IPage page)
     {
          _page = page;
     }

     public async Task Login(string userLogin, string userPassword)
     {
          await _accountDropdown.ClickAsync();
          await _usernameField.FillAsync(userLogin);
          await _passwordField.FillAsync(userPassword);
          await _submitButton.ClickAsync();
     }

     public async Task AddProduct(string productTitle)
     {
          await _latestProducts.GetByTitle(productTitle).ClickAsync();
          await _addProductButton.ClickAsync();
          await _cartQuantity.WaitForAsync();
     }

     public async Task ConfirmPurchase()
     {
          await _basketButton.ClickAsync();
          await _termsAgreedCheckbox.ClickAsync();
          await _confirmOrderButton.ClickAsync();
     }
}