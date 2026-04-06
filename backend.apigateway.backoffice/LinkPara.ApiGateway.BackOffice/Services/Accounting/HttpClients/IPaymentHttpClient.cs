using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;

public interface IPaymentHttpClient
{
    Task<ActionResult<PaginatedList<AccountingPaymentDto>>> GetListPaymentsAsync(GetFilterPaymentRequest request);
    Task<ActionResult<AccountingPaymentDto>> GetByIdAsync(Guid id);
    Task SavePaymentAsync(SavePaymentRequest request);
    Task DeletePaymentAsync(Guid id);
}
