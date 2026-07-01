using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Maps the UpdateSale API request to the application command.
/// The sale identifier is provided from the route and set by the controller.
/// </summary>
public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleItemRequest, UpdateSaleItemCommand>();
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(d => d.Id, o => o.Ignore());
    }
}
