using LinkPara.HttpProviders.Accounting.Models;

namespace LinkPara.HttpProviders.Accounting;

public interface IInvoiceService
{
    Task<InvoiceDto> GetInvoiceAsync(Guid clientReferenceId);
}