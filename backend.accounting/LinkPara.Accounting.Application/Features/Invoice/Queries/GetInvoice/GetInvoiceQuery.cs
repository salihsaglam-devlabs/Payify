using LinkPara.Accounting.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Invoice.Queries.GetInvoice;

public class GetInvoiceQuery : IRequest<InvoiceDto>
{
    public Guid PaymentClientReferenceId { get; set; }
}

public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto>
{
    private readonly IInvoiceService _billService;

    public GetInvoiceQueryHandler(IInvoiceService billService)
    {
        _billService = billService;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        return await _billService.GetPaymentBillAsync(request, cancellationToken);
    }
}