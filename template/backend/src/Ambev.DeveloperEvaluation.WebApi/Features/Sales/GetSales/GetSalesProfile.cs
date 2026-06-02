using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

public class GetSalesProfile : Profile
{
    public GetSalesProfile()
    {
        CreateMap<GetSalesRequest, GetSalesCommand>()
            .ForMember(d => d.Page, opt => opt.MapFrom(s => s._page ?? 1))
            .ForMember(d => d.Size, opt => opt.MapFrom(s => s._size ?? 10))
            .ForMember(d => d.Order, opt => opt.MapFrom(s => s._order));

        CreateMap<GetSalesResult, GetSalesResponse>();
    }
}
