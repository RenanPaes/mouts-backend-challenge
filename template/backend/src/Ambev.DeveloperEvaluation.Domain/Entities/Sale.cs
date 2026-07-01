using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale record in the system.
/// Following DDD, references to other domains (customer, branch, product) use the
/// External Identities pattern with denormalization of their descriptions.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>
    /// Gets the human-readable, business-facing sale number.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets the date when the sale was made.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Gets the external identifier of the customer.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets the denormalized customer description (External Identities pattern).
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the external identifier of the branch where the sale was made.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets the denormalized branch description (External Identities pattern).
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the total amount of the sale (sum of the non-cancelled item totals).
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the whole sale has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Gets the date and time when the sale was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of the last update to the sale.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the collection of items that compose this sale.
    /// </summary>
    public List<SaleItem> Items { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Sale"/> class.
    /// </summary>
    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an item to the sale, applying the discount business rules and
    /// recalculating the sale total.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="DomainException">Thrown when the sale is cancelled or the item breaks a business rule.</exception>
    public void AddItem(SaleItem item)
    {
        if (IsCancelled)
            throw new DomainException("Cannot add items to a cancelled sale.");

        item.ApplyDiscountRules();
        Items.Add(item);
        RecalculateTotal();
    }

    /// <summary>
    /// Cancels a single item within the sale and recalculates the sale total.
    /// </summary>
    /// <param name="itemId">The identifier of the item to cancel.</param>
    /// <returns>The cancelled item.</returns>
    /// <exception cref="DomainException">Thrown when the item does not exist or is already cancelled.</exception>
    public SaleItem CancelItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} does not belong to sale {Id}.");

        if (item.IsCancelled)
            throw new DomainException($"Item {itemId} is already cancelled.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    /// <summary>
    /// Cancels the entire sale together with all of its items.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the sale is already cancelled.</exception>
    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        IsCancelled = true;
        foreach (var item in Items)
            item.Cancel();

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates <see cref="TotalAmount"/> from the non-cancelled items.
    /// </summary>
    public void RecalculateTotal()
    {
        TotalAmount = Items.Where(i => !i.IsCancelled).Sum(i => i.Total);
    }

    /// <summary>
    /// Performs validation of the sale using <see cref="SaleValidator"/> rules.
    /// </summary>
    /// <returns>A <see cref="ValidationResultDetail"/> with the validation outcome.</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
