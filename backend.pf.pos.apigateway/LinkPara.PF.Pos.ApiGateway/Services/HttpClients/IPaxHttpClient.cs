using LinkPara.PF.Pos.ApiGateway.Models.Requests;
using LinkPara.PF.Pos.ApiGateway.Models.Responses;

namespace LinkPara.PF.Pos.ApiGateway.Services.HttpClients;

public interface IPaxHttpClient
{
    Task<TransactionResponse> PaxTransactionAsync(TransactionMerchantRequest request);
    Task<ParameterResponse> PaxParameterAsync(ParameterMerchantRequest request);
    Task<EndOfDayResponse> PaxEndOfDayAsync(EndOfDayMerchantRequest request);
    Task<ReconciliationResponse> PaxReconciliationAsync(ReconciliationMerchantRequest request);
}