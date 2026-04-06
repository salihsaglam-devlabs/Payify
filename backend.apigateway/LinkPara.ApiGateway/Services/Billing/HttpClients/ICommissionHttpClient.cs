using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.Services.Billing.HttpClients;

public interface ICommissionHttpClient
{
    Task<CommissionDto> GetByDetailAsync(CommissionFilterRequest request);
}