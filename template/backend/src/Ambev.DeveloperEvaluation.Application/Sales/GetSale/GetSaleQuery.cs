using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Query for retrieving a single sale by its identifier.
/// </summary>
public record GetSaleQuery : IRequest<SaleResult>
{
    public Guid Id { get; }

    public GetSaleQuery(Guid id)
    {
        Id = id;
    }
}
