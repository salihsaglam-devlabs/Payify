using LinkPara.HttpProviders.Fraud.Models;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IFraudService
{
    Task<bool> CheckFraudAsync(FraudTransactionDetail request, string fraudCommand, string clientIpAddress);
}