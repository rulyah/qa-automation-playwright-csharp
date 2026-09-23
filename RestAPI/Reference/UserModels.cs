using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlaywrightTests.RestAPI.Reference;

/// <summary>
/// Тіло запиту на створення або повну заміну користувача.
/// Свідомо БЕЗ id: ідентифікатор призначає сервер, клієнт його не вигадує.
/// </summary>
public sealed class UserPayload
{
    // [JsonPropertyName] задає точну назву поля в JSON.
    // Без нього ми залежали б від того, яку політику іменування
    // застосує серіалізатор, — а це неявний контракт, який легко зламати.
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("role")] public string Role { get; set; } = "user";
    [JsonPropertyName("active")] public bool Active { get; set; } = true;
}

/// <summary>
/// Те, що повертає сервер: ті самі поля плюс призначений ним id.
/// Окремий клас, а не спадкоємець UserPayload, — контракти запиту й
/// відповіді різні, і змішувати їх в одному типі означає рано чи пізно
/// випадково відправити на сервер зайве поле.
/// </summary>
public sealed class User
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleIdConverter))]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
    [JsonPropertyName("active")] public bool Active { get; set; }
}

/// <summary>
/// json-server віддає id числом (1, 2, 3), але якщо в базі з'явився хоча б один
/// рядковий id, наступні він генерує через nanoid — рядком ("mnkLOju").
/// Жорсткий int тут зламався б на рівному місці, тож читаємо обидва варіанти
/// і всередині тестів працюємо з id як з рядком.
/// </summary>
public sealed class FlexibleIdConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number => reader.GetInt64().ToString(),
            _ => throw new JsonException($"Неочікуваний тип id: {reader.TokenType}")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
