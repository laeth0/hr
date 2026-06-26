using Hr.BLL.DTOs.Addresses;
using Hr.BLL.DTOs.Employees;
using Hr.BLL.DTOs.Leaves;
using Hr.DAL.Models;
using Mapster;

namespace Hr.BLL.Mappings
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // All property names match by convention — Mapster handles them automatically.
            config.NewConfig<Employee, EmployeeDto>();
            config.NewConfig<Leave, LeaveDto>();
            config.NewConfig<Address, AddressDto>();

            // Source DTOs have fewer properties than the destination entities.
            // Unmatched fields (Id, nav properties, audit columns) keep their entity
            // defaults and are set explicitly in the service layer.
            config.NewConfig<CreateEmployeeDto, Employee>();
            config.NewConfig<CreateLeaveDto, Leave>();
            config.NewConfig<CreateAddressDto, Address>();
        }
    }
}
