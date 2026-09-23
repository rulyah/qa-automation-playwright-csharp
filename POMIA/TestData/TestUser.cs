namespace PlaywrightTests.POMIA.TestData;

/// <summary>
/// Тестові дані відокремлені від сторінок і від тестів:
/// сторінка знає "як залогінитись", тест — "ким", дані — "чим".
/// </summary>
public record TestUser(string Email, string Password)
{
    public static TestUser Default { get; } = new("user1@email.com", "demo12");
}
