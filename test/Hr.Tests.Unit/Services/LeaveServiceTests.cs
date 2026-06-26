using FluentAssertions;
using Hr.BLL.Common;
using Hr.BLL.DTOs.Leaves;
using Hr.BLL.Errors;
using Hr.BLL.Services;
using Hr.DAL.Enums;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Hr.Tests.Unit.Common;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Hr.Tests.Unit.Services;

public class LeaveServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILeaveRepository _leaveRepo;
    private readonly LeaveService _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public LeaveServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _employeeRepo = Substitute.For<IEmployeeRepository>();
        _leaveRepo = Substitute.For<ILeaveRepository>();
        _unitOfWork.Employees.Returns(_employeeRepo);
        _unitOfWork.Leaves.Returns(_leaveRepo);

        _sut = new LeaveService(_unitOfWork, TestMapper.Instance);
    }

    // ── GetByEmployeeAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmployeeAsync_EmployeeExists_ReturnsMappedLeaves()
    {
        var employeeId = Guid.NewGuid();
        var leaves = new[] { Builders.Leave(employeeId: employeeId), Builders.Leave(employeeId: employeeId) };
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _leaveRepo.GetByEmployeeAsync(employeeId, Arg.Any<CancellationToken>()).Returns(leaves);

        var result = await _sut.GetByEmployeeAsync(employeeId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByEmployeeAsync_EmployeeNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.GetByEmployeeAsync(employeeId);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingLeave_ReturnsSuccessWithDto()
    {
        var leave = Builders.Leave();
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.GetByIdAsync(leave.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(leave.Id);
        result.Value.Status.Should().Be(leave.Status);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentLeave_ReturnsNotFoundFailure()
    {
        var id = Guid.NewGuid();
        _leaveRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.GetByIdAsync(id);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.NotFound");
    }

    // ── RequestLeaveAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task RequestLeaveAsync_ValidAnnualLeave_CreatesLeaveAndReturnsSuccess()
    {
        var employee = Builders.Employee(allowedLeaveDays: 20);
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Annual, null);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.HasOverlappingLeaveAsync(employee.Id, dto.StartDate, dto.EndDate,
            cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _leaveRepo.GetByEmployeeAndYearAsync(employee.Id, dto.StartDate.Year, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.RequestLeaveAsync(employee.Id, dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmployeeId.Should().Be(employee.Id);
        result.Value.Status.Should().Be(LeaveStatus.Pending);
        await _leaveRepo.Received(1).AddAsync(Arg.Is<Leave>(l => l.EmployeeId == employee.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RequestLeaveAsync_EmployeeNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Sick, null);
        _employeeRepo.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.RequestLeaveAsync(employeeId, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    [Fact]
    public async Task RequestLeaveAsync_OverlappingDates_ReturnsDateOverlapFailure()
    {
        var employee = Builders.Employee();
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Annual, null);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.HasOverlappingLeaveAsync(employee.Id, dto.StartDate, dto.EndDate,
            cancellationToken: Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.RequestLeaveAsync(employee.Id, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LeaveErrors.DateOverlap);
    }

    [Fact]
    public async Task RequestLeaveAsync_AnnualLeaveExceedsBalance_ReturnsInsufficientBalanceFailure()
    {
        var employee = Builders.Employee(allowedLeaveDays: 5);
        // 5 days already approved
        var approvedLeave = Builders.Leave(
            employeeId: employee.Id,
            startDate: new DateOnly(Today.Year, 1, 1),
            endDate: new DateOnly(Today.Year, 1, 5),
            type: LeaveType.Annual,
            status: LeaveStatus.Approved);
        // requesting 3 more days — total 8 > 5 allowed
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Annual, null);

        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.HasOverlappingLeaveAsync(employee.Id, dto.StartDate, dto.EndDate,
            cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _leaveRepo.GetByEmployeeAndYearAsync(employee.Id, dto.StartDate.Year, Arg.Any<CancellationToken>())
            .Returns(new[] { approvedLeave });

        var result = await _sut.RequestLeaveAsync(employee.Id, dto);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.InsufficientBalance");
    }

    [Fact]
    public async Task RequestLeaveAsync_SickLeave_DoesNotCheckAnnualQuota()
    {
        var employee = Builders.Employee(allowedLeaveDays: 0);
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(5), LeaveType.Sick, "Flu");
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.HasOverlappingLeaveAsync(employee.Id, dto.StartDate, dto.EndDate,
            cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.RequestLeaveAsync(employee.Id, dto);

        result.IsSuccess.Should().BeTrue();
        await _leaveRepo.DidNotReceive().GetByEmployeeAndYearAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestLeaveAsync_AnnualLeaveWithinBalance_CreatesLeave()
    {
        var employee = Builders.Employee(allowedLeaveDays: 20);
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Annual, null);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.HasOverlappingLeaveAsync(employee.Id, dto.StartDate, dto.EndDate,
            cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _leaveRepo.GetByEmployeeAndYearAsync(employee.Id, dto.StartDate.Year, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.RequestLeaveAsync(employee.Id, dto);

        result.IsSuccess.Should().BeTrue();
    }

    // ── ApproveLeaveAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveLeaveAsync_PendingLeave_SetsApprovedStatusAndReturnsSuccess()
    {
        var managerId = Guid.NewGuid();
        var leave = Builders.Leave(status: LeaveStatus.Pending);
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.ApproveLeaveAsync(leave.Id, managerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LeaveStatus.Approved);
        result.Value.ReviewedByManagerId.Should().Be(managerId);
        _unitOfWork.Leaves.Received(1).Update(leave);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ApproveLeaveAsync_NonExistentLeave_ReturnsNotFoundFailure()
    {
        var id = Guid.NewGuid();
        _leaveRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.ApproveLeaveAsync(id, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.NotFound");
    }

    [Theory]
    [InlineData(LeaveStatus.Approved)]
    [InlineData(LeaveStatus.Rejected)]
    [InlineData(LeaveStatus.Cancelled)]
    public async Task ApproveLeaveAsync_NonPendingLeave_ReturnsNotPendingFailure(LeaveStatus status)
    {
        var leave = Builders.Leave(status: status);
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.ApproveLeaveAsync(leave.Id, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.NotPending");
    }

    // ── RejectLeaveAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RejectLeaveAsync_PendingLeave_SetsRejectedStatusAndReturnsSuccess()
    {
        var managerId = Guid.NewGuid();
        var leave = Builders.Leave(status: LeaveStatus.Pending);
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.RejectLeaveAsync(leave.Id, managerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LeaveStatus.Rejected);
        result.Value.ReviewedByManagerId.Should().Be(managerId);
    }

    [Theory]
    [InlineData(LeaveStatus.Approved)]
    [InlineData(LeaveStatus.Rejected)]
    [InlineData(LeaveStatus.Cancelled)]
    public async Task RejectLeaveAsync_NonPendingLeave_ReturnsNotPendingFailure(LeaveStatus status)
    {
        var leave = Builders.Leave(status: status);
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.RejectLeaveAsync(leave.Id, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.NotPending");
    }

    // ── CancelLeaveAsync ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(LeaveStatus.Pending)]
    [InlineData(LeaveStatus.Approved)]
    public async Task CancelLeaveAsync_CancellableStatus_SetsCancelledAndReturnsSuccess(LeaveStatus status)
    {
        var leave = Builders.Leave(status: status);
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.CancelLeaveAsync(leave.Id);

        result.IsSuccess.Should().BeTrue();
        leave.Status.Should().Be(LeaveStatus.Cancelled);
    }

    [Theory]
    [InlineData(LeaveStatus.Rejected)]
    [InlineData(LeaveStatus.Cancelled)]
    public async Task CancelLeaveAsync_AlreadyTerminalStatus_ReturnsCannotCancelFailure(LeaveStatus status)
    {
        var leave = Builders.Leave(status: status);
        _leaveRepo.GetByIdAsync(leave.Id, Arg.Any<CancellationToken>()).Returns(leave);

        var result = await _sut.CancelLeaveAsync(leave.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.CannotCancel");
    }

    [Fact]
    public async Task CancelLeaveAsync_NonExistentLeave_ReturnsNotFoundFailure()
    {
        var id = Guid.NewGuid();
        _leaveRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.CancelLeaveAsync(id);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Leave.NotFound");
    }

    // ── GetRemainingLeaveDaysAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetRemainingLeaveDaysAsync_NoLeavesUsed_ReturnsFullAllowance()
    {
        var employee = Builders.Employee(allowedLeaveDays: 20);
        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.GetByEmployeeAndYearAsync(employee.Id, Today.Year, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.GetRemainingLeaveDaysAsync(employee.Id, LeaveType.Annual, Today.Year);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task GetRemainingLeaveDaysAsync_SomeLeavesApproved_DeductsApprovedDaysOnly()
    {
        var employee = Builders.Employee(allowedLeaveDays: 20);
        // 5 approved days
        var approved = Builders.Leave(
            employeeId: employee.Id,
            startDate: new DateOnly(Today.Year, 1, 1),
            endDate: new DateOnly(Today.Year, 1, 5),
            type: LeaveType.Annual,
            status: LeaveStatus.Approved);
        // 3 pending days — should NOT reduce remaining balance
        var pending = Builders.Leave(
            employeeId: employee.Id,
            startDate: new DateOnly(Today.Year, 2, 1),
            endDate: new DateOnly(Today.Year, 2, 3),
            type: LeaveType.Annual,
            status: LeaveStatus.Pending);

        _employeeRepo.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        _leaveRepo.GetByEmployeeAndYearAsync(employee.Id, Today.Year, Arg.Any<CancellationToken>())
            .Returns(new[] { approved, pending });

        var result = await _sut.GetRemainingLeaveDaysAsync(employee.Id, LeaveType.Annual, Today.Year);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15); // 20 - 5 approved
    }

    [Fact]
    public async Task GetRemainingLeaveDaysAsync_EmployeeNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _employeeRepo.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.GetRemainingLeaveDaysAsync(employeeId, LeaveType.Annual, Today.Year);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }
}
