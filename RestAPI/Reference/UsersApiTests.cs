using Microsoft.Playwright;

namespace PlaywrightTests.RestAPI.Reference;

/// <summary>
/// Еталонний набір CRUD-тестів до /users мок-сервера json-server.
///
/// Назви тестів побудовані як Метод_Умова_ОчікуванийРезультат — зі звіту про
/// падіння одразу зрозуміло, що саме зламалось, без відкривання коду.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
[Category("RestApi")]
public class UsersApiTests : ApiTestBase
{
    [Test]
    public async Task GetUser_WhenUserExists_ReturnsSameDataThatWasCreated()
    {
        // Arrange — тест сам готує свої дані і не покладається на вміст db.json
        User created = await CreateUserAsync();

        // Act
        IAPIResponse response = await Api.GetAsync($"/users/{created.Id}");

        // Assert
        await ShouldHaveStatusAsync(response, 200);
        User actual = await ReadAsync<User>(response);

        // Assert.Multiple виконує всі перевірки і показує ВСІ розбіжності одразу,
        // а не падає на першій. Інакше чиниш одну помилку — і вилазить наступна.
        Assert.Multiple(() =>
        {
            Assert.That(actual.Id, Is.EqualTo(created.Id));
            Assert.That(actual.Name, Is.EqualTo(created.Name));
            Assert.That(actual.Email, Is.EqualTo(created.Email));
            Assert.That(actual.Role, Is.EqualTo(created.Role));
            Assert.That(actual.Active, Is.EqualTo(created.Active));
        });
    }

    [Test]
    public async Task GetUser_WhenUserDoesNotExist_Returns404()
    {
        // Негативні перевірки цінніші за позитивні: саме на обробці помилок
        // API ламаються найчастіше.
        IAPIResponse response = await Api.GetAsync($"/users/{MissingUserId}");

        await ShouldHaveStatusAsync(response, 404);
    }

    [Test]
    public async Task CreateUser_WithValidPayload_Returns201AndAssignsId()
    {
        // Arrange
        string unique = Guid.NewGuid().ToString("N")[..8];
        UserPayload payload = new()
        {
            Name = $"created-{unique}",
            Email = $"created-{unique}@example.com",
            Role = "admin",
            Active = true
        };

        // Act
        IAPIResponse response = await Api.PostAsync("/users", new() { DataObject = payload });

        // Assert
        await ShouldHaveStatusAsync(response, 201);

        User created = await ReadAsync<User>(response);
        TrackForCleanup($"/users/{created.Id}");

        Assert.Multiple(() =>
        {
            Assert.That(created.Id, Is.Not.Empty, "сервер має сам призначити id");
            Assert.That(created.Name, Is.EqualTo(payload.Name));
            Assert.That(created.Email, Is.EqualTo(payload.Email));
            Assert.That(created.Role, Is.EqualTo(payload.Role));
            Assert.That(created.Active, Is.EqualTo(payload.Active));

            // Заголовки — теж частина контракту. 201 зобов'язує повернути
            // Location з адресою створеного ресурсу; Playwright віддає
            // назви заголовків у нижньому регістрі.
            Assert.That(response.Headers, Does.ContainKey("location"),
                "POST має повертати заголовок Location");
        });
    }

    [Test]
    public async Task ReplaceUser_WithFullPayload_UpdatesEveryFieldButKeepsId()
    {
        // Arrange
        User created = await CreateUserAsync(role: "user", active: true);
        UserPayload replacement = new()
        {
            Name = "replaced",
            Email = "replaced@example.com",
            Role = "admin",
            Active = false
        };

        // Act
        IAPIResponse response = await Api.PutAsync($"/users/{created.Id}", new() { DataObject = replacement });

        // Assert
        await ShouldHaveStatusAsync(response, 200);
        User actual = await ReadAsync<User>(response);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Name, Is.EqualTo(replacement.Name));
            Assert.That(actual.Email, Is.EqualTo(replacement.Email));
            Assert.That(actual.Role, Is.EqualTo(replacement.Role));
            Assert.That(actual.Active, Is.EqualTo(replacement.Active));

            // id — це адреса ресурсу, а не його вміст. PUT не може її змінити,
            // навіть якщо надіслати інший id у тілі.
            Assert.That(actual.Id, Is.EqualTo(created.Id), "PUT не змінює id ресурсу");
        });
    }

    [Test]
    public async Task ReplaceUser_WithPartialPayload_WipesOmittedFields()
    {
        // Цей тест фіксує семантику PUT: він замінює ресурс ЦІЛКОМ, і поля,
        // яких немає в тілі, зникають. Поведінка неочевидна, тому вона має бути
        // закріплена тестом, а не жити в чиїйсь голові.
        User created = await CreateUserAsync();
        const string newEmail = "only-email@example.com";

        IAPIResponse response = await Api.PutAsync($"/users/{created.Id}",
            new() { DataObject = new { email = newEmail } });

        await ShouldHaveStatusAsync(response, 200);
        User actual = await ReadAsync<User>(response);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Email, Is.EqualTo(newEmail));
            Assert.That(actual.Name, Is.Empty, "name не передавали — PUT його стер");
            Assert.That(actual.Role, Is.Empty, "role не передавали — PUT його стер");
        });
    }

    [Test]
    public async Task UpdateUser_WithPatch_ChangesOnlyGivenField()
    {
        // Arrange
        User created = await CreateUserAsync();
        const string newEmail = "patched@example.com";

        // Анонімний об'єкт, а не UserPayload: у тілі PATCH має бути рівно те,
        // що ми змінюємо. Готова модель потягла б за собою решту полів зі
        // значеннями за замовчуванням і мовчки їх перезаписала.
        IAPIResponse response = await Api.PatchAsync($"/users/{created.Id}",
            new() { DataObject = new { email = newEmail } });

        // Assert
        await ShouldHaveStatusAsync(response, 200);
        User actual = await ReadAsync<User>(response);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Email, Is.EqualTo(newEmail));

            // Найважливіша частина перевірки PATCH — те, що НЕ змінилось.
            // Без цих трьох рядків тест пройшов би і тоді, коли PATCH знищив
            // решту полів.
            Assert.That(actual.Name, Is.EqualTo(created.Name));
            Assert.That(actual.Role, Is.EqualTo(created.Role));
            Assert.That(actual.Active, Is.EqualTo(created.Active));
        });
    }

    [Test]
    public async Task UpdateUser_WhenUserDoesNotExist_Returns404()
    {
        IAPIResponse response = await Api.PatchAsync($"/users/{MissingUserId}",
            new() { DataObject = new { email = "nobody@example.com" } });

        await ShouldHaveStatusAsync(response, 404);
    }

    [Test]
    public async Task DeleteUser_WhenUserExists_RemovesItFromCollection()
    {
        // Arrange
        User created = await CreateUserAsync();

        // Act
        IAPIResponse deleteResponse = await Api.DeleteAsync($"/users/{created.Id}");

        // Assert
        await ShouldHaveStatusAsync(deleteResponse, 200);

        // Перевіряємо наслідок операції, а не лише код відповіді: 200 означає
        // «сервер прийняв запит», і тільки повторний GET доводить, що запис
        // справді зник.
        IAPIResponse getResponse = await Api.GetAsync($"/users/{created.Id}");
        await ShouldHaveStatusAsync(getResponse, 404);
    }

    [Test]
    public async Task DeleteUser_WhenAlreadyDeleted_Returns404()
    {
        User created = await CreateUserAsync();
        await Api.DeleteAsync($"/users/{created.Id}");

        IAPIResponse response = await Api.DeleteAsync($"/users/{created.Id}");

        await ShouldHaveStatusAsync(response, 404);
    }
}
