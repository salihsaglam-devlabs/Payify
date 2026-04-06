using LinkPara.HttpProviders.Vault;
using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.CheckTransactionApproval;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartClientTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartOneTouchTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.UpdateActivationPINByCustomerIdTransaction;
using LinkPara.SoftOtp.Infrastructure.Integration.Connector;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LinkPara.SoftOtp.Infrastructure.Integration;

public class PowerFactorAdapter : IPowerFactorAdapter
{
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<PowerFactorAdapter> _logger;

    public PowerFactorAdapter(IVaultClient vaultClient, ILogger<PowerFactorAdapter> logger)
    {
        _vaultClient = vaultClient;
        _logger = logger;
    }
    public Task<GenerateActivationOtpResponse> GetActivationOtpAsync(GenerateActivationOtpRequest request)
    {
        var multifactorAuthConfig = _vaultClient.GetSecretValue<PowerfactorConfig>("/SoftOTPSecrets", "MultifactorAuth");
        request.ApplicationName = multifactorAuthConfig.ApplicationName;

        var response = new GenerateActivationOtpResponse();
        var returnObj = Executer.Post<GenerateActivationOtpResponse, GenerateActivationOtpRequest>(request, "GetActivationOtp", multifactorAuthConfig);
        response.Otp = returnObj.Otp;
        response.Results.AddRange(returnObj.Results);
        return Task.FromResult(response);
    }

    public Task<VerifyLoginOtpResponse> VerifyLoginOtpAsync(VerifyLoginOtpRequest request)
    {
        var multifactorAuthConfig = _vaultClient.GetSecretValue<PowerfactorConfig>("/SoftOTPSecrets", "MultifactorAuth");
        request.ApplicationNames = new List<string> { multifactorAuthConfig.ApplicationName };

        var response = Executer.Post<VerifyLoginOtpResponse, VerifyLoginOtpRequest>(request, "VerifyLoginOtp", multifactorAuthConfig);
        return Task.FromResult(response);
    }

    public Task<StartClientTransactionResponse> StartClientTransactionAsync(StartClientTransactionCommand command)
    {
        var multifactorAuthConfig = _vaultClient.GetSecretValue<PowerfactorConfig>("/SoftOTPSecrets", "MultifactorAuth");
        command.ApplicationName = multifactorAuthConfig.ApplicationName;

        var response =
            Executer.Post<StartClientTransactionResponse, StartClientTransactionCommand>(command,
                "StartClientTransaction", multifactorAuthConfig);

        return Task.FromResult(response);
    }

    public Task<CheckTransactionApprovalResponse> CheckTransactionApproval(CheckTransactionApprovalCommand command)
    {
        var multifactorAuthConfig = _vaultClient.GetSecretValue<PowerfactorConfig>("/SoftOTPSecrets", "MultifactorAuth");

        var response = Executer.Post<CheckTransactionApprovalResponse, CheckTransactionApprovalCommand>(command,
            "CheckTransactionApproval", multifactorAuthConfig);

        return Task.FromResult(response);
    }

    public Task<StartOneTouchTransactionResponse> StartOneTouchTransaction(StartOneTouchTransactionCommand command)
    {
        var multifactorAuthConfig = _vaultClient.GetSecretValue<PowerfactorConfig>("/SoftOTPSecrets", "MultifactorAuth");
        command.TransactionDefinitionKey = multifactorAuthConfig.TransactionDefinitionKey;

        var response = Executer.Post<StartOneTouchTransactionResponse, StartOneTouchTransactionCommand>(command,
            "StartOneTouchTransaction", multifactorAuthConfig);

        return Task.FromResult(response);
    }

    public Task<UpdateActivationPINByCustomerIdResponse> UpdateActivationPINByCustomerId(UpdateActivationPINByCustomerIdCommand command)
    {
        var multifactorAuthConfig = _vaultClient.GetSecretValue<PowerfactorConfig>("/SoftOTPSecrets", "MultifactorAuth");

        var response = Executer.PostBackOffice<UpdateActivationPINByCustomerIdResponse>(multifactorAuthConfig, command, "UpdateActivationPINByCustomerId");
        return Task.FromResult(response);
    }
}