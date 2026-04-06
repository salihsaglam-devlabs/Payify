using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.CheckTransactionApproval;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartClientTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartOneTouchTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.UpdateActivationPINByCustomerIdTransaction;

namespace LinkPara.SoftOtp.Application.Common.Interfaces;

public interface IPowerFactorAdapter
{
    public Task<GenerateActivationOtpResponse> GetActivationOtpAsync(GenerateActivationOtpRequest request);
    public Task<VerifyLoginOtpResponse> VerifyLoginOtpAsync(VerifyLoginOtpRequest request);
    public Task<StartClientTransactionResponse> StartClientTransactionAsync(StartClientTransactionCommand command);
    public Task<CheckTransactionApprovalResponse> CheckTransactionApproval(CheckTransactionApprovalCommand command);
    public Task<StartOneTouchTransactionResponse> StartOneTouchTransaction(StartOneTouchTransactionCommand command);
    public Task<UpdateActivationPINByCustomerIdResponse> UpdateActivationPINByCustomerId(UpdateActivationPINByCustomerIdCommand command);
}