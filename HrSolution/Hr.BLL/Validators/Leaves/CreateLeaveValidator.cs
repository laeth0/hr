using FluentValidation;
using Hr.BLL.DTOs.Leaves;

namespace Hr.BLL.Validators.Leaves
{
    public class CreateLeaveValidator : AbstractValidator<CreateLeaveDto>
    {
        public CreateLeaveValidator()
        {
            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
                .WithMessage("Start date cannot be in the past.");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be on or after the start date.");

            RuleFor(x => x.Type)
                .IsInEnum();
        }
    }
}
