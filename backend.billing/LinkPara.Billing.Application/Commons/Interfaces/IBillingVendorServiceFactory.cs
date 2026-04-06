namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IBillingVendorServiceFactory
{
    public Task<IBillingVendorService> GetBillingServiceAsync(Guid vendorId);
    public Task<bool> IsReconciliationCloseNeededAsync(Guid vendorId);
}