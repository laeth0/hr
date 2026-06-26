using FluentAssertions;
using Hr.BLL.DTOs.Employees;
using Hr.BLL.Validators.Employees;
using Hr.DAL.Enums;

namespace Hr.Tests.Unit.Validators;

public class UpdateEmployeeValidatorTests
{
    private readonly UpdateEmployeeValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_PassesAllRules()
    {
        var dto = new UpdateEmployeeDto("Alice Smith", 5_000m, EmploymentStatus.Active, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsWithValidationError(string name)
    {
        var dto = new UpdateEmployeeDto(name, 5_000m, EmploymentStatus.Active, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_FailsWithValidationError()
    {
        var dto = new UpdateEmployeeDto(new string('A', 201), 5_000m, EmploymentStatus.Active, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Fact]
    public void Validate_NegativeSalary_FailsWithValidationError()
    {
        var dto = new UpdateEmployeeDto("Alice", -1m, EmploymentStatus.Active, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Salary));
    }

    [Fact]
    public void Validate_NegativeAllowedLeaveDays_FailsWithValidationError()
    {
        var dto = new UpdateEmployeeDto("Alice", 5_000m, EmploymentStatus.Active, -1, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.AllowedLeaveDayPerYear));
    }

    [Fact]
    public void Validate_InvalidStatusValue_FailsWithValidationError()
    {
        var dto = new UpdateEmployeeDto("Alice", 5_000m, (EmploymentStatus)99, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Status));
    }

    [Theory]
    [InlineData(EmploymentStatus.Active)]
    [InlineData(EmploymentStatus.OnLeave)]
    [InlineData(EmploymentStatus.Suspended)]
    [InlineData(EmploymentStatus.Terminated)]
    public void Validate_AllValidStatuses_PassAllRules(EmploymentStatus status)
    {
        var dto = new UpdateEmployeeDto("Alice", 5_000m, status, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
