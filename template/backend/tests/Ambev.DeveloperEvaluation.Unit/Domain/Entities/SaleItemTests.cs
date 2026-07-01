using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the <see cref="SaleItem"/> entity, focused on the
/// quantity-based discount business rules.
/// </summary>
public class SaleItemTests
{
    [Theory(DisplayName = "Quantities below 4 receive no discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Given_QuantityBelowFour_When_ApplyingDiscount_Then_NoDiscount(int quantity)
    {
        // Arrange
        var item = SaleTestData.GenerateItem(quantity, unitPrice: 100m);

        // Act
        item.ApplyDiscountRules();

        // Assert
        item.DiscountPercentage.Should().Be(0m);
        item.Discount.Should().Be(0m);
        item.Total.Should().Be(quantity * 100m);
    }

    [Theory(DisplayName = "Quantities from 4 to 9 receive a 10% discount")]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void Given_QuantityBetweenFourAndNine_When_ApplyingDiscount_Then_TenPercent(int quantity)
    {
        // Arrange
        var item = SaleTestData.GenerateItem(quantity, unitPrice: 100m);

        // Act
        item.ApplyDiscountRules();

        // Assert
        item.DiscountPercentage.Should().Be(0.10m);
        item.Discount.Should().Be(quantity * 100m * 0.10m);
        item.Total.Should().Be(quantity * 100m * 0.90m);
    }

    [Theory(DisplayName = "Quantities from 10 to 20 receive a 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Given_QuantityBetweenTenAndTwenty_When_ApplyingDiscount_Then_TwentyPercent(int quantity)
    {
        // Arrange
        var item = SaleTestData.GenerateItem(quantity, unitPrice: 100m);

        // Act
        item.ApplyDiscountRules();

        // Assert
        item.DiscountPercentage.Should().Be(0.20m);
        item.Discount.Should().Be(quantity * 100m * 0.20m);
        item.Total.Should().Be(quantity * 100m * 0.80m);
    }

    [Fact(DisplayName = "Selling more than 20 identical items is not allowed")]
    public void Given_QuantityAboveTwenty_When_ApplyingDiscount_Then_ThrowsDomainException()
    {
        // Arrange
        var item = SaleTestData.GenerateItem(21, unitPrice: 100m);

        // Act
        var act = () => item.ApplyDiscountRules();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*more than 20 identical items*");
    }

    [Fact(DisplayName = "Cancelling an item marks it as cancelled")]
    public void Given_Item_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var item = SaleTestData.GenerateItem(5);
        item.ApplyDiscountRules();

        // Act
        item.Cancel();

        // Assert
        item.IsCancelled.Should().BeTrue();
    }
}
