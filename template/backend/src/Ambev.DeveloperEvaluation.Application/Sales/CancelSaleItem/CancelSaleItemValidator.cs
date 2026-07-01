using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Validator for <see cref="CancelSaleItemCommand"/>.
/// </summary>
public class CancelSaleItemValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty().WithMessage("Sale id is required.");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("Item id is required.");
    }
}
