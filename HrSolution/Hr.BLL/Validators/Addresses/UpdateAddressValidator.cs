using FluentValidation;
using Hr.BLL.DTOs.Addresses;

namespace Hr.BLL.Validators.Addresses
{
    public class UpdateAddressValidator : AbstractValidator<UpdateAddressDto>
    {
        public UpdateAddressValidator()
        {
            RuleFor(x => x.Street)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
