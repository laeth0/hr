using FluentAssertions;
using Hr.BLL.Common;
using Hr.BLL.DTOs.Addresses;
using Hr.BLL.Errors;
using Hr.BLL.Services;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using Hr.Tests.Unit.Common;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Hr.Tests.Unit.Services;

public class AddressServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly AddressService _sut;

    public AddressServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _employeeRepo = Substitute.For<IEmployeeRepository>();
        _addressRepo = Substitute.For<IAddressRepository>();
        _unitOfWork.Employees.Returns(_employeeRepo);
        _unitOfWork.Addresses.Returns(_addressRepo);

        _sut = new AddressService(_unitOfWork, TestMapper.Instance);
    }

    // ── GetByEmployeeAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmployeeAsync_EmployeeAndAddressExist_ReturnsMappedAddressDto()
    {
        var employeeId = Guid.NewGuid();
        var address = Builders.Address(employeeId: employeeId);
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(address);

        var result = await _sut.GetByEmployeeAsync(employeeId);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmployeeId.Should().Be(employeeId);
        result.Value.Street.Should().Be(address.Street);
        result.Value.City.Should().Be(address.City);
    }

    [Fact]
    public async Task GetByEmployeeAsync_EmployeeNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.GetByEmployeeAsync(employeeId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetByEmployeeAsync_EmployeeExistsButNoAddress_ReturnsAddressNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.GetByEmployeeAsync(employeeId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.NotFound");
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_EmployeeExistsNoExistingAddress_CreatesAndReturnsAddress()
    {
        var employeeId = Guid.NewGuid();
        var dto = new CreateAddressDto("10 Elm St", "Shelbyville");
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.CreateAsync(employeeId, dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmployeeId.Should().Be(employeeId);
        result.Value.Street.Should().Be(dto.Street);
        result.Value.City.Should().Be(dto.City);
        result.Value.Id.Should().NotBeEmpty();
        await _addressRepo.Received(1).AddAsync(
            Arg.Is<Address>(a => a.EmployeeId == employeeId && a.Street == dto.Street),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_EmployeeNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateAsync(employeeId, new CreateAddressDto("St", "City"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Employee.NotFound");
    }

    [Fact]
    public async Task CreateAsync_AddressAlreadyExists_ReturnsAlreadyExistsFailure()
    {
        var employeeId = Guid.NewGuid();
        var existingAddress = Builders.Address(employeeId: employeeId);
        _employeeRepo.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(existingAddress);

        var result = await _sut.CreateAsync(employeeId, new CreateAddressDto("New St", "New City"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.AlreadyExists");
        await _addressRepo.DidNotReceive().AddAsync(Arg.Any<Address>(), Arg.Any<CancellationToken>());
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingAddress_UpdatesStreetAndCityAndReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var address = Builders.Address(employeeId: employeeId, street: "Old St", city: "Old City");
        var dto = new UpdateAddressDto("New St", "New City");
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(address);

        var result = await _sut.UpdateAsync(employeeId, dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Street.Should().Be(dto.Street);
        result.Value.City.Should().Be(dto.City);
        _unitOfWork.Addresses.Received(1).Update(address);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAsync_AddressNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.UpdateAsync(employeeId, new UpdateAddressDto("St", "City"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.NotFound");
        _unitOfWork.Addresses.DidNotReceive().Update(Arg.Any<Address>());
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingAddress_DeletesAndReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var address = Builders.Address(employeeId: employeeId);
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(address);

        var result = await _sut.DeleteAsync(employeeId);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Addresses.Received(1).Remove(address);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_AddressNotFound_ReturnsNotFoundFailure()
    {
        var employeeId = Guid.NewGuid();
        _addressRepo.GetByEmployeeIdAsync(employeeId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.DeleteAsync(employeeId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.NotFound");
        _unitOfWork.Addresses.DidNotReceive().Remove(Arg.Any<Address>());
    }
}
