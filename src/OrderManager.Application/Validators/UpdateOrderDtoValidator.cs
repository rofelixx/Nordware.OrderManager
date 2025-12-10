using FluentValidation;
using OrderManager.Application.Dto;

namespace OrderManager.Application.Validators
{
    public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderDtoValidator()
        {
            RuleFor(x => x.ShippingAddress).NotEmpty();
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);

            RuleForEach(x => x.Items).SetValidator(new UpdateOrderItemDtoValidator());
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
