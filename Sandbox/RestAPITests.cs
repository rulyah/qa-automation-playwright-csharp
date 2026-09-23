using Microsoft.Playwright;

namespace PlaywrightTests.RestAPI;

[TestFixture]
public class RestAPITests
{
    IPlaywright _playwright;
    IAPIRequestContext _api;
    
    [SetUp]
    public async Task SetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _api = await _playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = "http://localhost:3000"
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        _playwright.Dispose();
    }
    
    [Test]
    public async Task GETTest()
    {
        IAPIResponse response = await _api.GetAsync("/users/1");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task POSTTest()
    {
        UserData newUser = new UserData
        {
            id = 4,
            name = "put test",
            email = "testemail@gmail.com",
            role = "user",
            active = true
        };
        
        APIRequestContextOptions options = new() { DataObject = newUser };
        IAPIResponse response = await _api.PostAsync("/users", options);
        Assert.That(response.Status, Is.EqualTo(201));
    }

    [Test]
    public async Task PutTest()
    {
        UserData newUser = new UserData
        {
            id = 4,
            name = "Adam",
            email = "admin@gmail.com",
            role = "admin",
            active = true
        };
        
        APIRequestContextOptions options = new() { DataObject = newUser };
        IAPIResponse response = await _api.PutAsync("/users/4", options);
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task DeleteTest()
    {
        IAPIResponse response = await _api.DeleteAsync("/users/4");
        Assert.That(response.Status, Is.EqualTo(200));
    }
    
    public class UserData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public bool active { get; set; }
    }
}