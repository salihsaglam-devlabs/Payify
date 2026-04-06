using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.HttpClients;

public interface ICustomerHttpClient
{
    Task<ActionResult<AccountingCustomerDto>> GetByIdAsync(Guid id);
    Task<ActionResult<PaginatedList<AccountingCustomerDto>>> GetListCustomersAsync(GetFilterCustomerRequest request);
    Task SaveCustomerAsync(SaveCustomerRequest request);
}
