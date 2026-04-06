using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Brand;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Publisher;
using LinkPara.Epin.Application.Features.Brands;
using LinkPara.Epin.Application.Features.Brands.Queries.GetFilterBrands;
using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IBrandService
{
    Task AddOrUpdateBrands(List<BrandServiceDto> brands);
    Task<List<BrandServiceDto>> GetAllBrandsFromService(List<PublisherServiceDto> publishers);
    Task<PaginatedList<BrandDto>> GetFilterBrandsAsync(GetFilterBrandsQuery request);
    Task<Brand> GetBrandByIdAsync(Guid id);
}
