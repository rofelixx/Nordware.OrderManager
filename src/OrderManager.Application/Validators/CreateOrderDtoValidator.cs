using FluentValidation;
using OrderManager.Application.Dto;

namespace OrderManager.Application.Validators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("O campo CustomerId é obrigatório.");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
                .MaximumLength(150).WithMessage("O nome do cliente deve ter no máximo 150 caracteres.");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("O email do cliente é obrigatório.")
                .MaximumLength(150).WithMessage("O email do cliente deve ter no máximo 150 caracteres.");

            RuleFor(x => x.CustomerCep)
                .MaximumLength(300).WithMessage("O Cep de entrega deve ter no máximo 300 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.CustomerCep));

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("O pedido deve possuir ao menos 1 item.")
                .Must(items => items.Count > 0).WithMessage("A lista de itens não pode estar vazia.");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateOrderItemDtoValidator());
        }
    }
}
