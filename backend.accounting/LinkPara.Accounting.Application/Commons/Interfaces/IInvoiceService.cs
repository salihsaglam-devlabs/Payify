using LinkPara.Accounting.Application.Features.Invoice;
using LinkPara.Accounting.Application.Features.Invoice.Queries.GetInvoice;

namespace LinkPara.Accounting.Application.Commons.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> GetPaymentBillAsync(GetInvoiceQuery request, CancellationToken cancellationToken);
}