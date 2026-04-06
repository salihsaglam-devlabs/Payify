using LinkPara.Epin.Application.Features.Brands.Queries.GetFilterBrands;
using LinkPara.Epin.Application.Features.Brands;
using LinkPara.SharedModels.Pagination;
using MediatR;
using LinkPara.Epin.Application.Commons.Interfaces;

namespace LinkPara.Epin.Application.Features.Products.Queries.GetFilterProducts;

public class GetFilterProductsQuery : SearchQueryParams, IRequest<List<ProductDto>>
{
    public Guid PublisherId { get; set; }
    public Guid BrandId { get; set; }
}

public class GetFilterProductsQueryHandler : IRequestHandler<GetFilterProductsQuery, List<ProductDto>>
{
    private readonly IProductService _productService;

    public GetFilterProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }
    public async Task<List<ProductDto>> Handle(GetFilterProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productService.GetFilterProductsAsync(request);
    }
}