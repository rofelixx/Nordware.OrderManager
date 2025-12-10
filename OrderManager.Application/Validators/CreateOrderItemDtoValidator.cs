using FluentValidation;
using OrderManager.Application.Dto;

namespace OrderManager.Application.Validators
{
    public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("O SKU é obrigatório.")
                .MaximumLength(50).WithMessage("O SKU deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do produto é obrigatório.")
                .MaximumLength(150).WithMessage("O nome do produto deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");
        }
    }
}
