using FluentAssertions;
using Hr.BLL.DTOs.Addresses;
using Hr.BLL.Validators.Addresses;

namespace Hr.Tests.Unit.Validators;

public class CreateAddressValidatorTests
{
    private readonly CreateAddressValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_PassesAllRules()
    {
        var dto = new CreateAddressDto("123 Main St", "Springfield");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyStreet_FailsWithValidationError(string street)
    {
        var dto = new CreateAddressDto(street, "Springfield");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Street));
    }

    [Fact]
    public void Validate_StreetExceedsMaxLength_FailsWithValidationError()
    {
        var dto = new CreateAddressDto(new string('A', 301), "Springfield");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Street));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyCity_FailsWithValidationError(string city)
    {
        var dto = new CreateAddressDto("123 Main St", city);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.City));
    }

    [Fact]
    public void Validate_CityExceedsMaxLength_FailsWithValidationError()
    {
        var dto = new CreateAddressDto("123 Main St", new string('A', 101));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.City));
    }
}

public class UpdateAddressValidatorTests
{
    private readonly UpdateAddressValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_PassesAllRules()
    {
        var dto = new UpdateAddressDto("456 Oak Ave", "Shelbyville");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyStreet_FailsWithValidationError(string street)
    {
        var dto = new UpdateAddressDto(street, "Shelbyville");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Street));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyCity_FailsWithValidationError(string city)
    {
        var dto = new UpdateAddressDto("456 Oak Ave", city);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.City));
    }

    [Fact]
    public void Validate_StreetExceedsMaxLength_FailsWithValidationError()
    {
        var dto = new UpdateAddressDto(new string('A', 301), "Shelbyville");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Street));
    }

    [Fact]
    public void Validate_CityExceedsMaxLength_FailsWithValidationError()
    {
        var dto = new UpdateAddressDto("456 Oak Ave", new string('A', 101));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.City));
    }
}
