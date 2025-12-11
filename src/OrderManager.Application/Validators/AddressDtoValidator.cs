using FluentValidation;
using OrderManager.Application.Dto;

namespace OrderManager.Application.Validators
{
    public class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            RuleFor(x => x.Cep).NotEmpty().Length(8, 9);
            RuleFor(x => x.Street).NotEmpty();
            RuleFor(x => x.Number).NotEmpty();
            RuleFor(x => x.Neighborhood).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.State).NotEmpty().Length(2);
        }
    }
}
