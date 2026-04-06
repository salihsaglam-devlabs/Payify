using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;

namespace LinkPara.Epin.Infrastructure.Consumers.CronJobs;

public class ProductConsumer : IConsumer<SyncEpinProducts>
{
    private readonly IPublisherService _publisherService;
    private readonly IBrandService _brandService;
    private readonly IProductService _productService;
    public ProductConsumer(IPublisherService publisherService, 
        IBrandService brandService,
        IProductService productService)
    {
        _publisherService = publisherService;
        _brandService = brandService;
        _productService = productService;
    }

    public async Task Consume(ConsumeContext<SyncEpinProducts> context)
    {
        var publishers = await _publisherService.GetPublishersFromServiceAsync();
        await _publisherService.AddOrUpdatePublishers(publishers);

        var brands = await _brandService.GetAllBrandsFromService(publishers);
        await _brandService.AddOrUpdateBrands(brands);

        List<ProductServiceDto> products = await _productService.GetAllProductsFromService(brands);
        await _productService.AddOrUpdateProducts(products);
    }
}
