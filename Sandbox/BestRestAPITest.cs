using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

namespace PlaywrightTests.RestAPI;

[TestFixture]
public class BestRestAPITest
{
    IPlaywright _playwright;
    IAPIRequestContext _api;

    List<string> _createdResources = new ();

    [OneTimeSetUp]
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
        for (int i = _createdResources.Count - 1; i >= 0; i--)
            await _api.DeleteAsync("/users/" + _createdResources[i]);
        
        await _api.DisposeAsync();
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
            Name = "put test",
            Email = "testemail@gmail.com",
            Role = "user",
            Active = true
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
            Name = "Adam",
            Email = "admin@gmail.com",
            Role = "admin",
            Active = true
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

    async Task<User> CreateUserAsync()
    {
        UserData newUser = new UserData
        {
            Name = "put test",
            Email = "testemail@gmail.com",
            Role = "user",
            Active = true
        };

        APIRequestContextOptions options = new() { DataObject = newUser };
        IAPIResponse response = await _api.PostAsync("/users", options);
        string responseText = await response.TextAsync();
        User? created = JsonSerializer.Deserialize<User>(responseText);
        return created ?? throw new AssertionException($"Не вдалось розпарсити відповідь: '{responseText}'");
    }
    
    public class User : UserData
    {
        [JsonPropertyName("id")] public string Id { get; set; }
    }

    public class UserData
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("email")] public string Email { get; set; }
        [JsonPropertyName("role")] public string Role { get; set; }
        [JsonPropertyName("active")] public bool Active { get; set; }
    }
}