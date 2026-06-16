using Application.DTOs.Product;
using FluentValidation;

namespace Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters.");
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters.");
    }
}
