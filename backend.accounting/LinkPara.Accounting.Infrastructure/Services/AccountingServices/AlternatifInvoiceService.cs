using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Features.Invoice;
using LinkPara.Accounting.Application.Features.Invoice.Queries.GetInvoice;

namespace LinkPara.Accounting.Infrastructure.Services.AccountingServices;

public class AlternatifInvoiceService : IInvoiceService
{
    public AlternatifInvoiceService()
    {
    }

    public Task<InvoiceDto> GetPaymentBillAsync(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
