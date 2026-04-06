using LinkPara.Accounting.Application.Features.Invoice;
using LinkPara.Accounting.Application.Features.Invoice.Queries.GetInvoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Accounting.API.Controllers;

public class InvoiceController : ApiControllerBase
{
    [Authorize(Policy = "AccountingInvoice:ReadAll")]
    [HttpGet("{clientReferenceId}")]
    public async Task<InvoiceDto> GetByClientReferenceIdAsync([FromRoute] Guid clientReferenceId)
    {
        return await Mediator.Send(new GetInvoiceQuery { PaymentClientReferenceId = clientReferenceId });
    }
}