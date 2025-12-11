using FluentValidation.TestHelper;
using OrderManager.Application.Dto;
using OrderManager.Application.Validators;
using System.Collections.Generic;
using Xunit;


namespace OrderManager.Application.Tests.Validators
{
    public class UpdateOrderDtoValidatorTests
    {
        private readonly UpdateOrderDtoValidator _validator;

        public UpdateOrderDtoValidatorTests()
        {
            _validator = new UpdateOrderDtoValidator();
        }

        private UpdateOrderDto MakeValid()
        {
            return new UpdateOrderDto
            {
                CustomerName = "John Doe",
                ShippingAddress = new AddressDto
                {
                    Street = "Rua A",
                    Number = 123,
                    Neighborhood = "Centro",
                    City = "São Paulo",
                    State = "SP",
                    Cep = "12345000"
                },
                Items = new List<UpdateOrderItemDto>()
                {
                    new UpdateOrderItemDto
                    {
                        Sku = "ABC123",
                        Name = "Produto",
                        Quantity = 2,
                        UnitPrice = 10
                    }
                }
            };
        }

        [Fact]
        public void Should_Pass_When_Valid()
        {
            var model = MakeValid();
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Error_When_CustomerName_Is_Empty()
        {
            var model = MakeValid();
            model.CustomerName = "";
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        }

        [Fact]
        public void Should_Have_Error_When_ShippingAddress_Is_Null()
        {
            var model = MakeValid();
            model.ShippingAddress.Cep = null;
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ShippingAddress.Cep);
        }

        [Fact]
        public void Should_Have_Error_When_Items_Is_Empty()
        {
            var model = MakeValid();
            model.Items.Clear();
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Items);
        }

        [Fact]
        public void Should_Have_Error_When_Item_Invalid()
        {
            var model = MakeValid();
            model.Items[0].Quantity = 0;
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        }
    }
}
