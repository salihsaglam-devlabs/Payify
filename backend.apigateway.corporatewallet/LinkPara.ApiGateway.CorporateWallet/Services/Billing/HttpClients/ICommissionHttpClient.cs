using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public interface ICommissionHttpClient
{
    Task<CommissionDto> GetByDetailAsync(CommissionFilterRequest request);
}