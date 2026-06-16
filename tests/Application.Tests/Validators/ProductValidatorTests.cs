using Application.Validators;
using Application.DTOs.Product;
using FluentValidation.TestHelper;

namespace Application.Tests.Validators;

public class ProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Should_Pass_When_ProductName_IsValid()
    {
        var result = _validator.TestValidate(new CreateProductRequest("Widget Pro"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void Should_Fail_When_ProductName_IsTooShortOrEmpty(string name)
    {
        var result = _validator.TestValidate(new CreateProductRequest(name));
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Should_Fail_When_ProductName_ExceedsMaxLength()
    {
        var longName = new string('X', 256);
        var result = _validator.TestValidate(new CreateProductRequest(longName));
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }
}
