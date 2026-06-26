using FluentAssertions;
using Hr.BLL.DTOs.Employees;
using Hr.BLL.Validators.Employees;

namespace Hr.Tests.Unit.Validators;

public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_PassesAllRules()
    {
        var dto = new CreateEmployeeDto("Alice Smith", "alice@hr.com", 5_000m, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsWithValidationError(string name)
    {
        var dto = new CreateEmployeeDto(name, "alice@hr.com", 5_000m, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_FailsWithValidationError()
    {
        var dto = new CreateEmployeeDto(new string('A', 201), "alice@hr.com", 5_000m, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_InvalidEmail_FailsWithValidationError(string email)
    {
        var dto = new CreateEmployeeDto("Alice", email, 5_000m, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    [Fact]
    public void Validate_NegativeSalary_FailsWithValidationError()
    {
        var dto = new CreateEmployeeDto("Alice", "alice@hr.com", -1m, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Salary));
    }

    [Fact]
    public void Validate_ZeroSalary_IsValid()
    {
        var dto = new CreateEmployeeDto("Alice", "alice@hr.com", 0m, 20, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NegativeAllowedLeaveDays_FailsWithValidationError()
    {
        var dto = new CreateEmployeeDto("Alice", "alice@hr.com", 5_000m, -1, null);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.AllowedLeaveDayPerYear));
    }

    [Fact]
    public void Validate_WithValidManagerId_PassesAllRules()
    {
        var dto = new CreateEmployeeDto("Alice", "alice@hr.com", 5_000m, 20, Guid.NewGuid());

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
