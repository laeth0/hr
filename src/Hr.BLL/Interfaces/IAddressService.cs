using Hr.BLL.Common;
using Hr.BLL.DTOs.Addresses;
using Hr.DAL.Interfaces.MarkerInterfaces;

namespace Hr.BLL.Interfaces
{
    public interface IAddressService : IScopedService
    {
        Task<Result<AddressDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> CreateAsync(Guid employeeId, CreateAddressDto dto, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> UpdateAsync(Guid employeeId, UpdateAddressDto dto, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
