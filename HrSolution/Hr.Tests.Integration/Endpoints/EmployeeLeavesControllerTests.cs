using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Hr.BLL.DTOs.Leaves;
using Hr.DAL.Enums;
using Hr.DAL.Models;
using Hr.Tests.Integration.Fixtures;

namespace Hr.Tests.Integration.Endpoints;

public class EmployeeLeavesControllerTests : IClassFixture<HrWebApplicationFactory>, IAsyncLifetime
{
    private readonly HrWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeeLeavesControllerTests(HrWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    // ── GET /api/employees/{id}/leaves ───────────────────────────────────────

    [Fact]
    public async Task GetByEmployee_EmployeeExistsNoLeaves_Returns200WithEmptyArray()
    {
        var employee = await SeedEmployeeAsync();

        var response = await _client.GetAsync($"/api/employees/{employee.Id}/leaves");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LeaveDto[]>();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByEmployee_EmployeeNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/employees/{Guid.NewGuid()}/leaves");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByEmployee_EmployeeWithLeaves_Returns200WithLeaves()
    {
        var employee = await SeedEmployeeAsync();
        await _factory.SeedAsync(async db =>
        {
            db.Leaves.Add(MakeLeave(employee.Id, Today.AddDays(5), Today.AddDays(7)));
            await Task.CompletedTask;
        });

        var response = await _client.GetAsync($"/api/employees/{employee.Id}/leaves");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LeaveDto[]>();
        body.Should().HaveCount(1);
    }

    // ── POST /api/employees/{id}/leaves ──────────────────────────────────────

    [Fact]
    public async Task RequestLeave_ValidAnnualLeave_Returns201WithPendingLeave()
    {
        var employee = await SeedEmployeeAsync(allowedLeaveDays: 20);
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(5), LeaveType.Annual, "Vacation");

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/leaves", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<LeaveDto>();
        body!.Status.Should().Be(LeaveStatus.Pending);
        body.EmployeeId.Should().Be(employee.Id);
        body.Type.Should().Be(LeaveType.Annual);
    }

    [Fact]
    public async Task RequestLeave_EmployeeNotFound_Returns404()
    {
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Sick, null);

        var response = await _client.PostAsJsonAsync($"/api/employees/{Guid.NewGuid()}/leaves", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestLeave_OverlappingDates_Returns500WithDateOverlapError()
    {
        var employee = await SeedEmployeeAsync();
        await _factory.SeedAsync(async db =>
        {
            db.Leaves.Add(MakeLeave(employee.Id, Today.AddDays(3), Today.AddDays(7)));
            await Task.CompletedTask;
        });
        // New request overlaps the existing leave
        var dto = new CreateLeaveDto(Today.AddDays(5), Today.AddDays(9), LeaveType.Annual, null);

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/leaves", dto);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task RequestLeave_AnnualLeaveExceedsQuota_Returns500WithInsufficientBalanceError()
    {
        var employee = await SeedEmployeeAsync(allowedLeaveDays: 3);
        await _factory.SeedAsync(async db =>
        {
            // 3 days already approved — quota exhausted
            db.Leaves.Add(MakeLeave(employee.Id,
                new DateOnly(Today.Year, 1, 1),
                new DateOnly(Today.Year, 1, 3),
                status: LeaveStatus.Approved));
            await Task.CompletedTask;
        });
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(2), LeaveType.Annual, null);

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/leaves", dto);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task RequestLeave_InvalidDto_StartDateInPast_Returns400()
    {
        var employee = await SeedEmployeeAsync();
        var dto = new CreateLeaveDto(Today.AddDays(-5), Today.AddDays(-1), LeaveType.Annual, null);

        var response = await _client.PostAsJsonAsync($"/api/employees/{employee.Id}/leaves", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/employees/{id}/leaves/remaining-days ────────────────────────

    [Fact]
    public async Task GetRemainingDays_NoLeavesUsed_ReturnsFullAllowance()
    {
        var employee = await SeedEmployeeAsync(allowedLeaveDays: 20);

        var response = await _client.GetAsync(
            $"/api/employees/{employee.Id}/leaves/remaining-days?type=Annual&year={Today.Year}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var days = await response.Content.ReadFromJsonAsync<int>();
        days.Should().Be(20);
    }

    [Fact]
    public async Task GetRemainingDays_EmployeeNotFound_Returns404()
    {
        var response = await _client.GetAsync(
            $"/api/employees/{Guid.NewGuid()}/leaves/remaining-days?type=Annual&year={Today.Year}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Employee> SeedEmployeeAsync(int allowedLeaveDays = 20)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Test Employee",
            Email = $"{Guid.NewGuid():N}@hr.com",
            Salary = 5_000m,
            Status = EmploymentStatus.Active,
            AllowedLeaveDayPerYear = allowedLeaveDays
        };
        await _factory.SeedAsync(async db => { db.Employees.Add(employee); await Task.CompletedTask; });
        return employee;
    }

    private static Leave MakeLeave(
        Guid employeeId,
        DateOnly startDate,
        DateOnly endDate,
        LeaveType type = LeaveType.Annual,
        LeaveStatus status = LeaveStatus.Pending) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeId = employeeId,
        StartDate = startDate,
        EndDate = endDate,
        Type = type,
        Status = status
    };
}
