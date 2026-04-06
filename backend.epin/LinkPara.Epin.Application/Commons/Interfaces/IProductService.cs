
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Brand;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;
using LinkPara.Epin.Application.Features.Products;
using LinkPara.Epin.Application.Features.Products.Queries.GetFilterProducts;
using LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IProductService
{
    Task AddOrUpdateProducts(List<ProductServiceDto> products);
    Task<List<ProductServiceDto>> GetAllProductsFromService(List<BrandServiceDto> brands);
    Task<List<ProductDto>> GetFilterProductsAsync(GetFilterProductsQuery request);
    Task<ProductServiceDto> GetProductAsync(Guid publisherId, Guid brandId, int productId);
    Task<List<Product>> GetProductsAsync();
}
