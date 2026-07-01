using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Ambev.DeveloperEvaluation.Domain.Entities.Sale"/> aggregate,
/// covering totals calculation and cancellation behavior.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "Total amount is the sum of non-cancelled item totals")]
    public void Given_SaleWithItems_When_Created_Then_TotalMatchesItemsSum()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item1 = SaleTestData.GenerateItem(5, unitPrice: 100m);   // 500 - 10% = 450
        var item2 = SaleTestData.GenerateItem(10, unitPrice: 50m);   // 500 - 20% = 400

        // Act
        sale.AddItem(item1);
        sale.AddItem(item2);

        // Assert
        sale.TotalAmount.Should().Be(850m);
    }

    [Fact(DisplayName = "Adding an item to a cancelled sale is not allowed")]
    public void Given_CancelledSale_When_AddingItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(1);
        sale.Cancel();

        // Act
        var act = () => sale.AddItem(SaleTestData.GenerateValidItem());

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*cancelled sale*");
    }

    [Fact(DisplayName = "Cancelling a sale cancels all items and zeroes the total")]
    public void Given_Sale_When_Cancelled_Then_AllItemsCancelledAndTotalZero()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(3);

        // Act
        sale.Cancel();

        // Assert
        sale.IsCancelled.Should().BeTrue();
        sale.Items.Should().OnlyContain(i => i.IsCancelled);
        sale.TotalAmount.Should().Be(0m);
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Cancelling an already cancelled sale is not allowed")]
    public void Given_CancelledSale_When_CancelledAgain_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(1);
        sale.Cancel();

        // Act
        var act = () => sale.Cancel();

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*already cancelled*");
    }

    [Fact(DisplayName = "Cancelling a single item removes it from the sale total")]
    public void Given_SaleWithItems_When_ItemCancelled_Then_TotalExcludesThatItem()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item1 = SaleTestData.GenerateItem(5, unitPrice: 100m);   // 450
        var item2 = SaleTestData.GenerateItem(10, unitPrice: 50m);   // 400
        sale.AddItem(item1);
        sale.AddItem(item2);

        // Act
        sale.CancelItem(item1.Id);

        // Assert
        item1.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(400m);
    }

    [Fact(DisplayName = "Cancelling an item that does not belong to the sale is not allowed")]
    public void Given_Sale_When_CancellingUnknownItem_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(1);

        // Act
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*does not belong*");
    }

    [Fact(DisplayName = "Validation passes for a well-formed sale")]
    public void Given_ValidSale_When_Validated_Then_IsValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithItems(2);

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Validation fails when a sale has no items")]
    public void Given_SaleWithoutItems_When_Validated_Then_IsInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
