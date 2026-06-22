using Hr.BLL.Common;
using Hr.BLL.DTOs.Addresses;
using Hr.BLL.Errors;
using Hr.BLL.Interfaces;
using Hr.DAL.Interfaces.RepositoriesInterfaces;
using Hr.DAL.Models;
using MapsterMapper;

namespace Hr.BLL.Services
{
    public class AddressService(IUnitOfWork unitOfWork, IMapper mapper) : IAddressService
    {
        public async Task<Result<AddressDto>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken cancellationToken = default)
        {
            var employeeExists = await unitOfWork.Employees.ExistsAsync(employeeId, cancellationToken);
            if (!employeeExists)
                return Result.Failure<AddressDto>(EmployeeErrors.NotFound(employeeId));

            var address = await unitOfWork.Addresses.GetByEmployeeIdAsync(employeeId, cancellationToken);
            return address is null
                ? Result.Failure<AddressDto>(AddressErrors.NotFound(employeeId))
                : Result.Success(mapper.Map<AddressDto>(address));
        }

        public async Task<Result<AddressDto>> CreateAsync(
            Guid employeeId, CreateAddressDto dto, CancellationToken cancellationToken = default)
        {
            var employeeExists = await unitOfWork.Employees.ExistsAsync(employeeId, cancellationToken);
            if (!employeeExists)
                return Result.Failure<AddressDto>(EmployeeErrors.NotFound(employeeId));

            var existing = await unitOfWork.Addresses.GetByEmployeeIdAsync(employeeId, cancellationToken);
            if (existing is not null)
                return Result.Failure<AddressDto>(AddressErrors.AlreadyExists(employeeId));

            var address = mapper.Map<Address>(dto);
            address.Id = Guid.NewGuid();
            address.EmployeeId = employeeId;

            await unitOfWork.Addresses.AddAsync(address, cancellationToken);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<AddressDto>(address));
        }

        public async Task<Result<AddressDto>> UpdateAsync(
            Guid employeeId, UpdateAddressDto dto, CancellationToken cancellationToken = default)
        {
            var address = await unitOfWork.Addresses.GetByEmployeeIdAsync(employeeId, cancellationToken);
            if (address is null)
                return Result.Failure<AddressDto>(AddressErrors.NotFound(employeeId));

            address.Street = dto.Street;
            address.City = dto.City;

            unitOfWork.Addresses.Update(address);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(mapper.Map<AddressDto>(address));
        }

        public async Task<Result> DeleteAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var address = await unitOfWork.Addresses.GetByEmployeeIdAsync(employeeId, cancellationToken);
            if (address is null)
                return Result.Failure(AddressErrors.NotFound(employeeId));

            unitOfWork.Addresses.Remove(address);
            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
