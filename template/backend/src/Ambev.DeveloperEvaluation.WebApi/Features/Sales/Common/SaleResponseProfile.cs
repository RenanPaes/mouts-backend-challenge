using Ambev.DeveloperEvaluation.Application.Sales.Common;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

/// <summary>
/// Maps the application-layer sale results to the API response models.
/// </summary>
public class SaleResponseProfile : Profile
{
    public SaleResponseProfile()
    {
        CreateMap<SaleItemResult, SaleItemResponse>();
        CreateMap<SaleResult, SaleResponse>();
    }
}
