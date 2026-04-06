using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Brand;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;
using Entities = LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using LinkPara.SystemUser;
using LinkPara.SharedModels.Pagination;
using LinkPara.Epin.Application.Features.Products;
using LinkPara.Epin.Application.Features.Products.Queries.GetFilterProducts;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Application.Features.Brands;
using AutoMapper;
using System.Security.Policy;

namespace LinkPara.Epin.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IEpinHttpClient _httpClient;
    private readonly IGenericRepository<Entities.Brand> _brandRepository;
    private readonly IGenericRepository<Entities.Product> _productRepository;
    private readonly IGenericRepository<Entities.Publisher> _publisherRepository;
    private readonly IApplicationUserService _applicationUserService;

    public ProductService(IEpinHttpClient httpClient,
        IGenericRepository<Entities.Product> productRepository,
        IGenericRepository<Entities.Publisher> publisherRepository,
        IGenericRepository<Entities.Brand> brandRepository,
        IApplicationUserService applicationUserService)
    {
        _httpClient = httpClient;
        _productRepository = productRepository;
        _publisherRepository = publisherRepository;
        _brandRepository = brandRepository;
        _applicationUserService = applicationUserService;
    }

    public async Task AddOrUpdateProducts(List<ProductServiceDto> products)
    {
        var dbProducts = await _productRepository.GetAll().ToListAsync();
        var dbPublishers = await _publisherRepository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active).ToListAsync();
        var dbBrands = await _brandRepository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active).ToListAsync();

        foreach (var item in products)
        {
            var publisher = dbPublishers.FirstOrDefault(x => x.ExternalId == item.ExternalPublisherId);

            if (publisher is null)
            {
                throw new NotFoundException(nameof(publisher));
            }

            var brand = dbBrands.FirstOrDefault(x => x.ExternalId == item.ExternalBrandId);

            if (brand is null)
            {
                throw new NotFoundException(nameof(brand));
            }

            if (!dbProducts.Any(x => x.ExternalId == item.Id))
            {
                var entity = new Entities.Product
                {
                    ExternalId = item.Id,
                    Name = item.Name,
                    RecordStatus = RecordStatus.Active,
                    Price = item.Price,
                    UnitPrice = item.Unit_Price,
                    Vat = item.Vat,
                    PublisherId = publisher.Id,
                    BrandId = brand.Id,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    Equivalent = item.Equivalent,
                    Discount = item.Discount
                };
                await _productRepository.AddAsync(entity);
            }
            else
            {
                var product = dbProducts.FirstOrDefault(x => x.ExternalId == item.Id);

                if (product is null)
                {
                    throw new NotFoundException(nameof(product));
                }

                product.ExternalId = item.Id;
                product.Name = item.Name;
                product.RecordStatus = RecordStatus.Active;
                product.Price = item.Price;
                product.UnitPrice = item.Unit_Price;
                product.Vat = item.Vat;
                product.PublisherId = publisher.Id;
                product.BrandId = brand.Id;
                product.Equivalent = item.Equivalent;
                product.Discount = item.Discount;

                await _productRepository.UpdateAsync(product);

                dbProducts.Remove(product);
            }
        }

        foreach (var item in dbProducts)
        {
            item.RecordStatus = RecordStatus.Passive;
            await _productRepository.UpdateAsync(item);
        }
    }

    public async Task<List<ProductServiceDto>> GetAllProductsFromService(List<BrandServiceDto> brands)
    {
        var result = new List<ProductServiceDto>();
        foreach (var brand in brands)
        {
            var response = await _httpClient.GetProductsAsync(brand.ExternalPublisherId, brand.Id);
            response.Products.ForEach(x =>
            {
                x.ExternalPublisherId = brand.ExternalPublisherId;
                x.ExternalBrandId = brand.Id;
            });

            result.AddRange(response.Products);
        }
        return result;
    }

    public async Task<List<ProductDto>> GetFilterProductsAsync(GetFilterProductsQuery request)
    {
        var publisher = await _publisherRepository.GetAll().FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active && x.Id == request.PublisherId);
        var brand = await _brandRepository.GetAll().FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active && x.Id == request.BrandId);

        var response = await _httpClient.GetProductsAsync(publisher.ExternalId, brand.ExternalId);

        var products = response.Products.Select(x =>
            new ProductDto
            {
                Price = x.Price,
                Discount = x.Discount,
                Equivalent = x.Equivalent,
                Id = x.Id,
                Name = x.Name,
                UnitPrice = x.Unit_Price,
                Vat = x.Vat,
                PublisherId = request.PublisherId,
                BrandId = request.BrandId
            });

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            products = products.Where(x => x.Name.Contains(request.Q));
        }

        return products.ToList();
    }

    public async Task<ProductServiceDto> GetProductAsync(Guid publisherId, Guid brandId, int productId)
    {
        var publisher = await _publisherRepository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active && x.Id == publisherId).FirstOrDefaultAsync();
        var brand = await _brandRepository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active && x.Id == brandId).FirstOrDefaultAsync();

        var response = await _httpClient.GetProductsAsync(publisher.ExternalId, brand.ExternalId);

        return response.Products.FirstOrDefault(x => x.Id == productId);
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _productRepository.GetAll().ToListAsync();
    }
}
