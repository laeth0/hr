using FluentAssertions;
using Hr.BLL.DTOs.Leaves;
using Hr.BLL.Validators.Leaves;
using Hr.DAL.Enums;

namespace Hr.Tests.Unit.Validators;

public class CreateLeaveValidatorTests
{
    private readonly CreateLeaveValidator _validator = new();

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Fact]
    public void Validate_ValidFutureLeave_PassesAllRules()
    {
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(5), LeaveType.Annual, "Holiday");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_StartDateInPast_FailsWithValidationError()
    {
        var dto = new CreateLeaveDto(Today.AddDays(-1), Today.AddDays(3), LeaveType.Annual, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.StartDate));
    }

    [Fact]
    public void Validate_StartDateIsToday_PassesAllRules()
    {
        var dto = new CreateLeaveDto(Today, Today.AddDays(2), LeaveType.Sick, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_FailsWithValidationError()
    {
        var dto = new CreateLeaveDto(Today.AddDays(5), Today.AddDays(2), LeaveType.Annual, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.EndDate));
    }

    [Fact]
    public void Validate_StartAndEndSameDay_PassesAllRules()
    {
        var tomorrow = Today.AddDays(1);
        var dto = new CreateLeaveDto(tomorrow, tomorrow, LeaveType.Sick, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidLeaveType_FailsWithValidationError()
    {
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), (LeaveType)99, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Type));
    }

    [Theory]
    [InlineData(LeaveType.Annual)]
    [InlineData(LeaveType.Sick)]
    [InlineData(LeaveType.Unpaid)]
    [InlineData(LeaveType.Other)]
    public void Validate_AllValidLeaveTypes_PassAllRules(LeaveType type)
    {
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), type, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullReason_IsAllowed()
    {
        var dto = new CreateLeaveDto(Today.AddDays(1), Today.AddDays(3), LeaveType.Annual, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
