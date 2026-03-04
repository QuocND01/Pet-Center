using ProductAPI.DTOs;
using FluentValidation;

namespace ProductAPI.Validation
{
    public class CreateProductValidator : AbstractValidator<CreateProductDTO>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Can not be empty")
                .Matches(@"^[\p{L}0-9 ]+$")
                .WithMessage("Can not contain special characters");

            RuleFor(x => x.ProductDescription)
                .NotEmpty().WithMessage("Can not be empty")
                .Matches(@"^[\p{L}0-9 ]+$")
                .WithMessage("Can not contain special characters");

            RuleFor(x => x.ProductPrice)
                .GreaterThan(0)
                .WithMessage("Price must greater than 0");

            RuleFor(x => x.StockQuantity)
               .GreaterThan(0)
               .WithMessage("Quantity must greater than 0");
        }
    }
}
