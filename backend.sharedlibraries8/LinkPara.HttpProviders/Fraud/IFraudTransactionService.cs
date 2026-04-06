using LinkPara.HttpProviders.Fraud.Models;

namespace LinkPara.HttpProviders.Fraud;

public interface IFraudTransactionService
{
    Task<TransactionResponse> ExecuteTransaction(FraudCheckRequest request);
}
