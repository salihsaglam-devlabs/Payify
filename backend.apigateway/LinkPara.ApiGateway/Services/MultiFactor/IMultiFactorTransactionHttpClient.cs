using LinkPara.ApiGateway.Commons.MultiFactorModels;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

namespace LinkPara.ApiGateway.Services.MultiFactor;

public interface IMultiFactorTransactionHttpClient
{
    public Task<StartClientTransactionResponse> StartClientTransaction(StartClientTransactionRequest request);
    public Task<CheckTransactionApprovalResponse> CheckTransactionApproval(CheckTransactionApprovalRequest request);

    public Task<OneTouchTransactionResponse> StartOneTouchTransaction(StartOneTouchTransactionRequest request);
}
