using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates a <see cref="SaleItem"/>, enforcing the quantity-based business rules
/// so that invalid input is reported as a validation error rather than a runtime failure.
/// </summary>
public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(item => item.ProductTitle)
            .NotEmpty().WithMessage("ProductTitle is required.")
            .MaximumLength(200).WithMessage("ProductTitle cannot be longer than 200 characters.");

        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(SaleItem.MaxQuantity)
            .WithMessage($"It is not possible to sell more than {SaleItem.MaxQuantity} identical items.");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0).WithMessage("UnitPrice must be greater than 0.");
    }
}
