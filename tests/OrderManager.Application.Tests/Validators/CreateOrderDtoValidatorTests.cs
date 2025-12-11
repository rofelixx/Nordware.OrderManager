using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManager.Application.Dto;
using OrderManager.Application.Validators;
using System;
using System.Collections.Generic;
using Xunit;

namespace OrderManager.Application.Tests.Validators
{
    public class CreateOrderDtoValidatorTests
    {
        private readonly CreateOrderDtoValidator _validator;

        public CreateOrderDtoValidatorTests()
        {
            _validator = new CreateOrderDtoValidator();
        }

        private CreateOrderDto MakeValid()
        {
            return new CreateOrderDto
            {
                CustomerId = Guid.NewGuid(),
                CustomerName = "John Doe",
                CustomerEmail = "email@email.com",
                ShippingAddress = new AddressDto
                {
                    Street = "Rua A",
                    Number = 123,
                    Neighborhood = "Centro",
                    City = "Curitiba",
                    State = "PR",
                    Cep = "12345000"
                },
                Items = new List<CreateOrderItemDto>()
                {
                    new CreateOrderItemDto
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
        public void Should_Have_Error_When_CustomerEmail_Is_Invalid()
        {
            var model = MakeValid();
            model.CustomerEmail = "";
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
        }

        [Fact]
        public void Should_Have_Error_When_Cep_Is_Empty()
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
        public void Should_Have_Error_When_Item_Is_Invalid()
        {
            var model = MakeValid();
            model.Items[0].Quantity = 0;
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        }
    }
}