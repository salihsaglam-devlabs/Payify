using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

namespace LinkPara.HttpProviders.MultiFactor;

public interface IMultiFactorService
{
    public Task<GenerateActivationOtpResponse> SendActivationOtpAsync(ActivationOtpRequest request);
    public Task<VerifyLoginResponse> VerifyLoginOtpAsync(VerifyLoginRequest request);
    public Task<StartClientTransactionResponse> StartClientTransactionAsync(StartClientTransactionRequest command);
    public Task<CheckTransactionApprovalResponse> CheckTransactionApprovalAsync(CheckTransactionApprovalRequest command);
    public Task<StartOneTouchTransactionResponse> StartOneTouchTransactionAsync(StartOneTouchTransactionRequest command);
    public Task<UpdateActivationPINByCustomerIdResponse> UpdateActivationPINByCustomerIdAsync(UpdateActivationPINByCustomerIdRequest command);
}