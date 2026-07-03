using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Validator for <see cref="GetSaleQuery"/>.
/// </summary>
public class GetSaleValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Sale id is required.");
    }
}
