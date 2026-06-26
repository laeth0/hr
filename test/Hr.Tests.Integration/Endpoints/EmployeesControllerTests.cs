using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hr.BLL.DTOs.Employees;
using Hr.DAL.Enums;
using Hr.DAL.Models;
using Hr.Tests.Integration.Fixtures;

namespace Hr.Tests.Integration.Endpoints;

/// <summary>
/// End-to-end tests for the /api/employees endpoints.
/// Each test class gets its own factory (unique InMemory DB).
/// Tests within the class reset the DB in the constructor for isolation.
/// </summary>
public class EmployeesControllerTests : IClassFixture<HrWebApplicationFactory>, IAsyncLifetime
{
    private readonly HrWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeesControllerTests(HrWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // ── GET /api/employees ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WhenNoEmployees_Returns200WithEmptyArray()
    {
        var response = await _client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto[]>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenEmployeesExist_Returns200WithAllEmployees()
    {
        await _factory.SeedAsync(async db =>
        {
            db.Employees.AddRange(
                MakeEmployee("Alice", "alice@hr.com"),
                MakeEmployee("Bob", "bob@hr.com"));
            await Task.CompletedTask;
        });

        var response = await _client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto[]>();
        body.Should().HaveCount(2);
    }

    // ── GET /api/employees/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingEmployee_Returns200WithEmployee()
    {
        var employee = MakeEmployee("Alice", "alice@hr.com");
        await _factory.SeedAsync(async db =>
        {
            db.Employees.Add(employee);
            await Task.CompletedTask;
        });

        var response = await _client.GetAsync($"/api/employees/{employee.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        body!.Id.Should().Be(employee.Id);
        body.Name.Should().Be(employee.Name);
        body.Email.Should().Be(employee.Email);
    }

    [Fact]
    public async Task GetById_NonExistentEmployee_Returns404()
    {
        var response = await _client.GetAsync($"/api/employees/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/employees ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidDto_Returns201WithCreatedEmployee()
    {
        var dto = new CreateEmployeeDto("Carol White", "carol@hr.com", 6_000m, 20, null);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be(dto.Name);
        body.Email.Should().Be(dto.Email);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409Conflict()
    {
        var existing = MakeEmployee("Alice", "alice@hr.com");
        await _factory.SeedAsync(async db => { db.Employees.Add(existing); await Task.CompletedTask; });

        var dto = new CreateEmployeeDto("Alice 2", "alice@hr.com", 5_000m, 15, null);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_InvalidDto_EmptyName_Returns400()
    {
        var dto = new CreateEmployeeDto("", "alice@hr.com", 5_000m, 20, null);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidDto_BadEmail_Returns400()
    {
        var dto = new CreateEmployeeDto("Alice", "not-an-email", 5_000m, 20, null);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithValidManager_Returns201()
    {
        var manager = MakeEmployee("Manager", "mgr@hr.com");
        await _factory.SeedAsync(async db => { db.Employees.Add(manager); await Task.CompletedTask; });

        var dto = new CreateEmployeeDto("Report", "report@hr.com", 4_000m, 15, manager.Id);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        body!.ManagerId.Should().Be(manager.Id);
    }

    [Fact]
    public async Task Create_WithNonExistentManager_Returns404()
    {
        var dto = new CreateEmployeeDto("Report", "report@hr.com", 4_000m, 15, Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/employees/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingEmployee_Returns200WithUpdatedValues()
    {
        var employee = MakeEmployee("Alice", "alice@hr.com");
        await _factory.SeedAsync(async db => { db.Employees.Add(employee); await Task.CompletedTask; });

        var dto = new UpdateEmployeeDto("Alice Updated", 8_000m, EmploymentStatus.OnLeave, 25, null);

        var response = await _client.PutAsJsonAsync($"/api/employees/{employee.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        body!.Name.Should().Be(dto.Name);
        body.Salary.Should().Be(dto.Salary);
        body.Status.Should().Be(dto.Status);
    }

    [Fact]
    public async Task Update_NonExistentEmployee_Returns404()
    {
        var dto = new UpdateEmployeeDto("Name", 5_000m, EmploymentStatus.Active, 20, null);

        var response = await _client.PutAsJsonAsync($"/api/employees/{Guid.NewGuid()}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_InvalidDto_EmptyName_Returns400()
    {
        var employee = MakeEmployee("Alice", "alice@hr.com");
        await _factory.SeedAsync(async db => { db.Employees.Add(employee); await Task.CompletedTask; });

        var dto = new UpdateEmployeeDto("", 5_000m, EmploymentStatus.Active, 20, null);

        var response = await _client.PutAsJsonAsync($"/api/employees/{employee.Id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── DELETE /api/employees/{id} ───────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingEmployeeNoSubordinates_Returns204()
    {
        var employee = MakeEmployee("Alice", "alice@hr.com");
        await _factory.SeedAsync(async db => { db.Employees.Add(employee); await Task.CompletedTask; });

        var response = await _client.DeleteAsync($"/api/employees/{employee.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistentEmployee_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/employees/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_EmployeeWithSubordinates_Returns500WithHasSubordinatesError()
    {
        var manager = MakeEmployee("Manager", "mgr@hr.com");
        var report = MakeEmployee("Report", "report@hr.com", managerId: manager.Id);
        await _factory.SeedAsync(async db =>
        {
            db.Employees.Add(manager);
            db.Employees.Add(report);
            await Task.CompletedTask;
        });

        var response = await _client.DeleteAsync($"/api/employees/{manager.Id}");

        // HasSubordinates is a generic Failure error type → mapped to 500 by BaseController
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // ── GET /api/employees/{id}/subordinates ─────────────────────────────────

    [Fact]
    public async Task GetSubordinates_ExistingManagerWithReports_Returns200WithSubordinates()
    {
        var manager = MakeEmployee("Manager", "mgr@hr.com");
        var report1 = MakeEmployee("Report1", "r1@hr.com", managerId: manager.Id);
        var report2 = MakeEmployee("Report2", "r2@hr.com", managerId: manager.Id);
        await _factory.SeedAsync(async db =>
        {
            db.Employees.AddRange(manager, report1, report2);
            await Task.CompletedTask;
        });

        var response = await _client.GetAsync($"/api/employees/{manager.Id}/subordinates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeDto[]>();
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubordinates_NonExistentManager_Returns404()
    {
        var response = await _client.GetAsync($"/api/employees/{Guid.NewGuid()}/subordinates");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Employee MakeEmployee(
        string name,
        string email,
        Guid? managerId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Email = email,
        Salary = 5_000m,
        Status = EmploymentStatus.Active,
        AllowedLeaveDayPerYear = 20,
        ManagerId = managerId
    };
}
