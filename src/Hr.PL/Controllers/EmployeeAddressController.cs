using Hr.BLL.DTOs.Addresses;
using Hr.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hr.PL.Controllers
{
    [Route("api/employees/{employeeId:guid}/address")]
    public class EmployeeAddressController(IAddressService addressService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken cancellationToken)
        {
            var result = await addressService.GetByEmployeeAsync(employeeId, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Guid employeeId, CreateAddressDto dto, CancellationToken cancellationToken)
        {
            var result = await addressService.CreateAsync(employeeId, dto, cancellationToken);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetByEmployee), new { employeeId }, result.Value)
                : MapError(result.Error);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            Guid employeeId, UpdateAddressDto dto, CancellationToken cancellationToken)
        {
            var result = await addressService.UpdateAsync(employeeId, dto, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid employeeId, CancellationToken cancellationToken)
        {
            var result = await addressService.DeleteAsync(employeeId, cancellationToken);
            return NoContentOrProblem(result);
        }
    }
}
