using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a single product line within a <see cref="Sale"/>.
/// Uses the External Identities pattern: the referenced product is denormalized
/// (identifier plus a description snapshot and the unit price at the moment of sale).
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Maximum quantity of identical items allowed in a single line.
    /// </summary>
    public const int MaxQuantity = 20;

    /// <summary>
    /// Minimum quantity required to be eligible for any discount.
    /// </summary>
    public const int MinQuantityForDiscount = 4;

    /// <summary>
    /// Minimum quantity required for the higher (20%) discount tier.
    /// </summary>
    public const int MinQuantityForHigherDiscount = 10;

    /// <summary>
    /// Gets the identifier of the parent sale.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets the external identifier of the referenced product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets the denormalized product description (External Identities pattern).
    /// </summary>
    public string ProductTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets the quantity of identical products in this line.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets the unit price of the product at the moment of sale.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets the discount percentage applied to this line (0, 0.10 or 0.20).
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// Gets the total discount amount applied to this line.
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Gets the net total amount for this line (gross amount minus discount).
    /// </summary>
    public decimal Total { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this item has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Applies the quantity-based discount business rules and recalculates
    /// <see cref="DiscountPercentage"/>, <see cref="Discount"/> and <see cref="Total"/>.
    /// </summary>
    /// <remarks>
    /// Business rules:
    /// <list type="bullet">
    /// <item>Selling more than 20 identical items is not allowed.</item>
    /// <item>Quantities below 4 are not eligible for a discount.</item>
    /// <item>4 to 9 identical items receive a 10% discount.</item>
    /// <item>10 to 20 identical items receive a 20% discount.</item>
    /// </list>
    /// </remarks>
    /// <exception cref="DomainException">Thrown when quantity is above the allowed maximum.</exception>
    public void ApplyDiscountRules()
    {
        if (Quantity > MaxQuantity)
            throw new DomainException($"It is not possible to sell more than {MaxQuantity} identical items.");

        if (Quantity < 1)
            throw new DomainException("Quantity must be at least 1.");

        DiscountPercentage = Quantity switch
        {
            >= MinQuantityForHigherDiscount => 0.20m,
            >= MinQuantityForDiscount => 0.10m,
            _ => 0m
        };

        var grossAmount = UnitPrice * Quantity;
        Discount = grossAmount * DiscountPercentage;
        Total = grossAmount - Discount;
    }

    /// <summary>
    /// Marks this item as cancelled and zeroes its contribution to the sale total.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }

    /// <summary>
    /// Performs validation of the sale item using <see cref="SaleItemValidator"/> rules.
    /// </summary>
    /// <returns>A <see cref="ValidationResultDetail"/> with the validation outcome.</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleItemValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
