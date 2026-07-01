using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// AutoMapper profile for the CreateSale operation.
/// Item and total calculations are performed by the domain (via <c>Sale.AddItem</c>),
/// so the collection and computed members are ignored here.
/// </summary>
public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleItemCommand, SaleItem>();

        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(d => d.Items, o => o.Ignore());
    }
}
