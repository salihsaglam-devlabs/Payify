using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SystemUser;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.Services;

public class FraudService : IFraudService
{
    private readonly ILogger<FraudService> _logger;
    private readonly IParameterService _parameterService;
    private readonly IFraudTransactionService _transactionService;
    private readonly IVaultClient _vaultClient;
    private readonly IApplicationUserService _applicationUserService;
    
    public FraudService(
        ILogger<FraudService> logger, 
        IParameterService parameterService, 
        IFraudTransactionService transactionService,
        IVaultClient vaultClient,
        IApplicationUserService applicationUserService)
    {
        _logger = logger;
        _parameterService = parameterService;
        _transactionService = transactionService;
        _vaultClient = vaultClient;
        _applicationUserService = applicationUserService;
    }
    
    public async Task<bool> CheckFraudAsync(FraudTransactionDetail request, string fraudCommand, string clientIpAddress)
    {
        var isTransactionCheckEnabled = _vaultClient
            .GetSecretValue<bool>("SharedSecrets", "ServiceState", "PfTransactionEnabled");

        if (!isTransactionCheckEnabled)
        {
            return true;
        }

        var requestFraud = new FraudCheckRequest
        {
            CommandName = fraudCommand,
            ExecuteTransaction = request,
            UserId = _applicationUserService.ApplicationUserId.ToString(),
            Module = "PF",
            AccountKycLevel = AccountKycLevel.NoneKyc,
            CommandJson = JsonConvert.SerializeObject(request),
            ClientIpAddress = clientIpAddress
        };

        var transaction = new TransactionResponse();
        try
        {
            transaction = await _transactionService.ExecuteTransaction(requestFraud);
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"Fraud {fraudCommand} Error : {exception}");
        }

        if (transaction.IsSuccess)
        {
            int riskLevel;
            try
            {
                var parameter = await _parameterService.GetParameterAsync("FraudParameters", "RiskLevel");
                riskLevel = Convert.ToInt32(parameter.ParameterValue);
            }
            catch (Exception exception)
            {
                _logger.LogError($"GetParameterAsync Error : {exception} ");
                riskLevel = (int)RiskLevel.Critical;
            }

            if ((int)transaction.RiskLevel >= riskLevel)
            {
                return false;
            }
        }
        else
        {
            _logger.LogError($"Fraud {fraudCommand} Error : {transaction.ErrorMessage}");
        }
        return true;
    }
}