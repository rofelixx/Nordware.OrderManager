using FluentAssertions;
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

        [Fact]
        public void Should_Return_Error_When_CustomerId_Is_Invalid()
        {
            var dto = new CreateOrderDto
            {
                CustomerId = Guid.Empty,
                Items = new List<CreateOrderItemDto>()
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "CustomerId");
        }

        [Fact]
        public void Should_Return_Error_When_No_Items_Were_Provided()
        {
            var dto = new CreateOrderDto
            {
                CustomerId = Guid.Empty,
                Items = new List<CreateOrderItemDto>()
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Should_Pass_When_Dto_Is_Valid()
        {
            var dto = new CreateOrderDto
            {
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "email@mail.com",
                CustomerName = "Customer Name",
                CustomerCep = "123456",                            
                Items = new()
                {
                    new CreateOrderItemDto 
                    {
                        Name = "Item 1",
                        Sku = "SKU1",
                        UnitPrice = 10,
                        Quantity = 2
                    }
                }
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}