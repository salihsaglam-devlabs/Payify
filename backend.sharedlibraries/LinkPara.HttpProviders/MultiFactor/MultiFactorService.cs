using System.Net.Http.Json;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.MultiFactor;

public class MultiFactorService : HttpClientBase, IMultiFactorService
{
    public MultiFactorService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<GenerateActivationOtpResponse> SendActivationOtpAsync(ActivationOtpRequest request)
    {
        var response = await PostAsJsonAsync("v1/authentication/device-activation", request);

        var verifyResponse = await response.Content.ReadFromJsonAsync<GenerateActivationOtpResponse>();

        return verifyResponse;
    }

    public async Task<VerifyLoginResponse> VerifyLoginOtpAsync(VerifyLoginRequest request)
    {
        var response = await PostAsJsonAsync("v1/authentication/verify-login", request);

        var verifyResponse = await response.Content.ReadFromJsonAsync<VerifyLoginResponse>();

        return verifyResponse;
    }

    public async Task<StartClientTransactionResponse> StartClientTransactionAsync(StartClientTransactionRequest command)
    {
        var response = await PostAsJsonAsync("v1/transaction/start-client-transaction", command);

        var transactionResponse = await response.Content.ReadFromJsonAsync<StartClientTransactionResponse>();

        return transactionResponse;
    }

    public async Task<CheckTransactionApprovalResponse> CheckTransactionApprovalAsync(CheckTransactionApprovalRequest command)
    {
        var response = await PostAsJsonAsync("v1/transaction/check-client-transaction", command);

        var transactionResponse = await response.Content.ReadFromJsonAsync<CheckTransactionApprovalResponse>();

        return transactionResponse;
    }

    public async Task<StartOneTouchTransactionResponse> StartOneTouchTransactionAsync(StartOneTouchTransactionRequest command)
    {
        var response = await PostAsJsonAsync("v1/transaction/start-one-touch-transaction", command);

        var transactionResponse = await response.Content.ReadFromJsonAsync<StartOneTouchTransactionResponse>();

        return transactionResponse;
    }

    public async Task<UpdateActivationPINByCustomerIdResponse> UpdateActivationPINByCustomerIdAsync(UpdateActivationPINByCustomerIdRequest command)
    {
        var response = await PostAsJsonAsync("v1/authentication/update-activation-pin", command);

        var transactionResponse = await response.Content.ReadFromJsonAsync<UpdateActivationPINByCustomerIdResponse>();

        return transactionResponse;
    }
}