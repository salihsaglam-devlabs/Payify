using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;

public interface ICustomerTransactionService
{
    Task<CustomerTransactionResponse> GetTransactionsByCustomerTransactionIdAsync(CustomerTransactionRequest request);
}
