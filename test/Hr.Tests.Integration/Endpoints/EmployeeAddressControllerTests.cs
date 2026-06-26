using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hr.BLL.DTOs.Addresses;
using Hr.DAL.Enums;
using Hr.DAL.Models;
using Hr.Tests.Integration.Fixtures;

namespace Hr.Tests.Integration.Endpoints;

public class EmployeeAddressControllerTests : IClassFixture<HrWebApplicationFactory>, IAsyncLifetime
{
    private readonly HrWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeeAddressControllerTests(HrWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ── GET /api/employees/{id}/address ──────────────────────────────────────

    [Fact]
    public async Task GetByEmployee_EmployeeAndAddressExist_Returns200WithAddress()
    {
        var employee = await SeedEmployeeAsync();
        await _factory.SeedAsync(async db =>
        {
            db.Addresses.Add(MakeAddress(employee.Id, "10 Elm St", "Springfield"));
            await Task.CompletedTask;
        });

        var response = await _client.GetAsync($"/api/employees/{employee.Id}/address");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AddressDto>();
        body!.EmployeeId.Should().Be(employee.Id);
        body.Street.Should().Be("10 Elm St");
        body.City.Should().Be("Springfield");
    }

    [Fact]
    public async Task GetByEmployee_EmployeeNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/employees/{Guid.NewGuid()}/address");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByEmployee_EmployeeExistsNoAddress_Returns404()
    {
        var employee = await SeedEmployeeAsync();

        var response = await _client.GetAsync($"/api/employees/{employee.Id}/address");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/employees/{id}/address ─────────────────────────────────────

    [Fact]
    public async Task Create_ValidDto_Returns201WithCreatedAddress()
    {
        var employee = await SeedEmployeeAsync();
        var dto = new CreateAddressDto("42 Oak Ave", "Shelbyville");

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AddressDto>();
        body!.EmployeeId.Should().Be(employee.Id);
        body.Street.Should().Be(dto.Street);
        body.City.Should().Be(dto.City);
        body.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_EmployeeNotFound_Returns404()
    {
        var dto = new CreateAddressDto("42 Oak Ave", "Shelbyville");

        var response = await _client.PostAsJsonAsync($"/api/employees/{Guid.NewGuid()}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_AddressAlreadyExists_Returns409Conflict()
    {
        var employee = await SeedEmployeeAsync();
        await _factory.SeedAsync(async db =>
        {
            db.Addresses.Add(MakeAddress(employee.Id));
            await Task.CompletedTask;
        });
        var dto = new CreateAddressDto("New St", "New City");

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_InvalidDto_EmptyStreet_Returns400()
    {
        var employee = await SeedEmployeeAsync();
        var dto = new CreateAddressDto("", "Shelbyville");

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidDto_EmptyCity_Returns400()
    {
        var employee = await SeedEmployeeAsync();
        var dto = new CreateAddressDto("42 Oak Ave", "");

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT /api/employees/{id}/address ──────────────────────────────────────

    [Fact]
    public async Task Update_ExistingAddress_Returns200WithUpdatedValues()
    {
        var employee = await SeedEmployeeAsync();
        await _factory.SeedAsync(async db =>
        {
            db.Addresses.Add(MakeAddress(employee.Id, "Old St", "Old City"));
            await Task.CompletedTask;
        });
        var dto = new UpdateAddressDto("New St", "New City");

        var response = await _client.PutAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AddressDto>();
        body!.Street.Should().Be(dto.Street);
        body.City.Should().Be(dto.City);
    }

    [Fact]
    public async Task Update_AddressNotFound_Returns404()
    {
        var employee = await SeedEmployeeAsync();
        var dto = new UpdateAddressDto("New St", "New City");

        var response = await _client.PutAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_InvalidDto_EmptyStreet_Returns400()
    {
        var employee = await SeedEmployeeAsync();
        var dto = new UpdateAddressDto("", "New City");

        var response = await _client.PutAsJsonAsync($"/api/employees/{employee.Id}/address", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── DELETE /api/employees/{id}/address ───────────────────────────────────

    [Fact]
    public async Task Delete_ExistingAddress_Returns204()
    {
        var employee = await SeedEmployeeAsync();
        await _factory.SeedAsync(async db =>
        {
            db.Addresses.Add(MakeAddress(employee.Id));
            await Task.CompletedTask;
        });

        var response = await _client.DeleteAsync($"/api/employees/{employee.Id}/address");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_AddressNotFound_Returns404()
    {
        var employee = await SeedEmployeeAsync();

        var response = await _client.DeleteAsync($"/api/employees/{employee.Id}/address");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Employee> SeedEmployeeAsync()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Test Employee",
            Email = $"{Guid.NewGuid():N}@hr.com",
            Salary = 5_000m,
            Status = EmploymentStatus.Active,
            AllowedLeaveDayPerYear = 20
        };
        await _factory.SeedAsync(async db => { db.Employees.Add(employee); await Task.CompletedTask; });
        return employee;
    }

    private static Address MakeAddress(
        Guid employeeId,
        string street = "123 Main St",
        string city = "Springfield") => new()
    {
        Id = Guid.NewGuid(),
        EmployeeId = employeeId,
        Street = street,
        City = city
    };
}
