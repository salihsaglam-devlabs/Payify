using AutoMapper;
using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Epin.Application.Features.Brands.Queries.GetFilterBrands;

public class GetFilterBrandsQuery : SearchQueryParams, IRequest<PaginatedList<BrandDto>>
{
    public Guid? PublisherId { get; set; }
}

public class GetFilterBrandsQueryHandler : IRequestHandler<GetFilterBrandsQuery, PaginatedList<BrandDto>>
{
    private readonly IBrandService _brandService;

    public GetFilterBrandsQueryHandler(IBrandService brandService)
    {
        _brandService = brandService;
    }

    public async Task<PaginatedList<BrandDto>> Handle(GetFilterBrandsQuery request, CancellationToken cancellationToken)
    {
        return await _brandService.GetFilterBrandsAsync(request);
    }
}
