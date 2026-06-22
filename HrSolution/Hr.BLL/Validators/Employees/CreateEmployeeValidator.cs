using FluentValidation;
using Hr.BLL.DTOs.Employees;

namespace Hr.BLL.Validators.Employees
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.AllowedLeaveDayPerYear)
                .GreaterThanOrEqualTo(0);
        }
    }
}
