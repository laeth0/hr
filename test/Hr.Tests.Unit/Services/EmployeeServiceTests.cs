using FluentAssertions;
using Hr.BLL.Common;
using Hr.BLL.DTOs.Employees;
using Hr.BLL.Errors;
using Hr.BLL.Services;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Hr.Tests.Unit.Common;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Hr.Tests.Unit.Services;

public class EmployeeServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _employeeRepo = Substitute.For<IEmployeeRepository>();
        _unitOfWork.Employees.Returns(_employeeRepo);

        _sut = new EmployeeService(_unitOfWork, TestMapper.Instance);
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WhenEmployeesExist_ReturnsMappedDtos()
    {
        var employees = new[] { Builders.Employee(), Builders.Employee(email: "bob@hr.com") };
        _employeeRepo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(employees);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Should().BeOfType<EmployeeDto>());
    }

    [Fact]
    public async Task GetAllAsync_WhenNoEmployees_ReturnsEmptyCollection()
    {
        _employeeRepo.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsSuccessWithDto()
    {
        var employee = Builders.Employee();
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);

        var result = await _sut.GetByIdAsync(employee.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(employee.Id);
        result.Value.Name.Should().Be(employee.Name);
        result.Value.Email.Should().Be(employee.Email);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEmployee_ReturnsNotFoundFailure()
    {
        var id = Guid.NewGuid();
        _employeeRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.GetByIdAsync(id);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_UniqueEmailNoManager_ReturnsSuccessWithCreatedDto()
    {
        var dto = new CreateEmployeeDto("Bob Jones", "bob@hr.com", 4_000m, 15, null);
        _employeeRepo.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be(dto.Name);
        result.Value.Email.Should().Be(dto.Email);
        await _unitOfWork.Employees.Received(1).AddAsync(
            Arg.Is<Employee>(e => e.Email == dto.Email), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsEmailConflictFailure()
    {
        var dto = new CreateEmployeeDto("Bob Jones", "taken@hr.com", 4_000m, 15, null);
        _employeeRepo.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>())
            .Returns(Builders.Employee(email: dto.Email));

        var result = await _sut.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmployeeErrors.EmailConflict);
        await _unitOfWork.Employees.DidNotReceive().AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidManager_CreatesEmployeeSuccessfully()
    {
        var managerId = Guid.NewGuid();
        var dto = new CreateEmployeeDto("Bob Jones", "bob@hr.com", 4_000m, 15, managerId);
        _employeeRepo.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).ReturnsNull();
        _employeeRepo.ExistsAsync(managerId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.ManagerId.Should().Be(managerId);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentManager_ReturnsManagerNotFoundFailure()
    {
        var managerId = Guid.NewGuid();
        var dto = new CreateEmployeeDto("Bob Jones", "bob@hr.com", 4_000m, 15, managerId);
        _employeeRepo.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).ReturnsNull();
        _employeeRepo.ExistsAsync(managerId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateAsync(dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.ManagerNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingEmployee_UpdatesFieldsAndReturnsSuccess()
    {
        var employee = Builders.Employee();
        var dto = new UpdateEmployeeDto("Updated Name", 9_000m, EmploymentStatus.OnLeave, 25, null);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);

        var result = await _sut.UpdateAsync(employee.Id, dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(dto.Name);
        result.Value.Salary.Should().Be(dto.Salary);
        result.Value.Status.Should().Be(dto.Status);
        result.Value.AllowedLeaveDayPerYear.Should().Be(dto.AllowedLeaveDayPerYear);
        _unitOfWork.Employees.Received(1).Update(employee);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEmployee_ReturnsNotFoundFailure()
    {
        var id = Guid.NewGuid();
        _employeeRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.UpdateAsync(id, new UpdateEmployeeDto("X", 1m, EmploymentStatus.Active, 10, null));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_EmployeeSetsSelfAsManager_ReturnsSelfManagementFailure()
    {
        var employee = Builders.Employee();
        var dto = new UpdateEmployeeDto("Name", 5_000m, EmploymentStatus.Active, 20, employee.Id);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);

        var result = await _sut.UpdateAsync(employee.Id, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmployeeErrors.SelfManagement);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentManager_ReturnsManagerNotFoundFailure()
    {
        var employee = Builders.Employee();
        var newManagerId = Guid.NewGuid();
        var dto = new UpdateEmployeeDto("Name", 5_000m, EmploymentStatus.Active, 20, newManagerId);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _employeeRepo.ExistsAsync(newManagerId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.UpdateAsync(employee.Id, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.ManagerNotFound");
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_EmployeeWithNoSubordinates_DeletesAndReturnsSuccess()
    {
        var employee = Builders.Employee();
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _employeeRepo.GetSubordinatesAsync(employee.Id, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.DeleteAsync(employee.Id);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Employees.Received(1).Remove(employee);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentEmployee_ReturnsNotFoundFailure()
    {
        var id = Guid.NewGuid();
        _employeeRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.DeleteAsync(id);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_EmployeeWithSubordinates_ReturnsHasSubordinatesFailure()
    {
        var manager = Builders.Employee();
        var subordinate = Builders.Employee(managerId: manager.Id);
        _employeeRepo.GetByIdAsync(manager.Id, Arg.Any<CancellationToken>()).Returns(manager);
        _employeeRepo.GetSubordinatesAsync(manager.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { subordinate });

        var result = await _sut.DeleteAsync(manager.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmployeeErrors.HasSubordinates);
        _unitOfWork.Employees.DidNotReceive().Remove(Arg.Any<Employee>());
    }

    // ── GetSubordinatesAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetSubordinatesAsync_ExistingManager_ReturnsMappedSubordinates()
    {
        var managerId = Guid.NewGuid();
        var subordinates = new[] { Builders.Employee(managerId: managerId), Builders.Employee(managerId: managerId, email: "c@hr.com") };
        _employeeRepo.ExistsAsync(managerId, Arg.Any<CancellationToken>()).Returns(true);
        _employeeRepo.GetSubordinatesAsync(managerId, Arg.Any<CancellationToken>()).Returns(subordinates);

        var result = await _sut.GetSubordinatesAsync(managerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubordinatesAsync_NonExistentManager_ReturnsNotFoundFailure()
    {
        var managerId = Guid.NewGuid();
        _employeeRepo.ExistsAsync(managerId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.GetSubordinatesAsync(managerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
