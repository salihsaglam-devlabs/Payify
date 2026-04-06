using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Infrastructure.Services.BillingServices;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Infrastructure.Services.Billing;

public class BillingServiceFactory : IBillingVendorServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IGenericRepository<Vendor> _vendorRepository;

    public BillingServiceFactory(IServiceProvider serviceProvider, 
        IGenericRepository<Vendor> vendorRepository)
    {
        _serviceProvider = serviceProvider;
        _vendorRepository = vendorRepository;
    }

    public async Task<IBillingVendorService> GetBillingServiceAsync(Guid vendorId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);

        if (vendor is null)
        {
            throw new NotFoundException(nameof(Vendor),vendorId);
        }

        return vendor.Name switch
        {
            "SekerBank" => (IBillingVendorService)_serviceProvider.GetService(typeof(SekerBankBillingService)),
            "MockBank" => (IBillingVendorService)_serviceProvider.GetService(typeof(MockBillingService)),
            "VakifKatilim" => (IBillingVendorService)_serviceProvider.GetService(typeof(VakifKatilimBillingService)),
            _ => throw new InvalidOperationException($"InvalidPostingBatchLevelDetected: {vendor.Name}")
        };
    }
    
    public async Task<bool> IsReconciliationCloseNeededAsync(Guid vendorId)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId);

        if (vendor is null)
        {
            throw new NotFoundException(nameof(Vendor),vendorId);
        }
        return vendor.Name switch
        {
            "VakifKatilim" => true,
            _ => false
        };
    }
}