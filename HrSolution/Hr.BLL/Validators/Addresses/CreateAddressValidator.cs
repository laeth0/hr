using FluentValidation;
using Hr.BLL.DTOs.Addresses;

namespace Hr.BLL.Validators.Addresses
{
    public class CreateAddressValidator : AbstractValidator<CreateAddressDto>
    {
        public CreateAddressValidator()
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
