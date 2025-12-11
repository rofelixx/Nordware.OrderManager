using FluentValidation;
using OrderManager.Application.Dto;

namespace OrderManager.Application.Validators
{
    public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderDtoValidator()
        {
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);

            RuleFor(x => x.ShippingAddress)
              .NotNull()
              .SetValidator(new AddressDtoValidator());

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("O pedido deve possuir ao menos 1 item.")
                .Must(items => items.Count > 0).WithMessage("A lista de itens não pode estar vazia.");

            RuleForEach(x => x.Items)
                .SetValidator(new UpdateOrderItemDtoValidator());
        }
    }

    public class UpdateOrderItemDtoValidator : AbstractValidator<UpdateOrderItemDto>
    {
        public UpdateOrderItemDtoValidator()
        {
            RuleFor(x => x.Sku).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.UnitPrice).GreaterThan(0);
        }
    }
}
