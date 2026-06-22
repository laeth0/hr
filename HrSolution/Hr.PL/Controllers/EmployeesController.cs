using Hr.BLL.DTOs.Employees;
using Hr.BLL.Interfaces;
using Hr.PL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Hr.PL.Controllers
{
    [Route("api/employees")]
    public class EmployeesController(IEmployeeService employeeService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var employees = await employeeService.GetAllAsync(cancellationToken);
            return Ok(employees);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await employeeService.GetByIdAsync(id, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpPost]
        [TypeFilter(typeof(ValidationFilter<CreateEmployeeDto>))]
        public async Task<IActionResult> Create(CreateEmployeeDto dto, CancellationToken cancellationToken)
        {
            var result = await employeeService.CreateAsync(dto, cancellationToken);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
                : MapError(result.Error);
        }

        [HttpPut("{id:guid}")]
        [TypeFilter(typeof(ValidationFilter<UpdateEmployeeDto>))]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken)
        {
            var result = await employeeService.UpdateAsync(id, dto, cancellationToken);
            return OkOrProblem(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await employeeService.DeleteAsync(id, cancellationToken);
            return NoContentOrProblem(result);
        }

        [HttpGet("{id:guid}/subordinates")]
        public async Task<IActionResult> GetSubordinates(Guid id, CancellationToken cancellationToken)
        {
            var result = await employeeService.GetSubordinatesAsync(id, cancellationToken);
            return OkOrProblem(result);
        }
    }
}
