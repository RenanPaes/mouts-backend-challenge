using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validates a <see cref="Sale"/> aggregate, including its items.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty().WithMessage("SaleNumber is required.")
            .MaximumLength(50).WithMessage("SaleNumber cannot be longer than 50 characters.");

        RuleFor(sale => sale.SaleDate)
            .NotEmpty().WithMessage("SaleDate is required.");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required.")
            .MaximumLength(100).WithMessage("CustomerName cannot be longer than 100 characters.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("BranchId is required.");

        RuleFor(sale => sale.BranchName)
            .NotEmpty().WithMessage("BranchName is required.")
            .MaximumLength(100).WithMessage("BranchName cannot be longer than 100 characters.");

        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("A sale must contain at least one item.");

        RuleForEach(sale => sale.Items).SetValidator(new SaleItemValidator());
    }
}
