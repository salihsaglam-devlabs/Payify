using LinkPara.HttpProviders.MoneyTransfer.Models;

namespace LinkPara.HttpProviders.MoneyTransfer;

public interface IMoneyTransferService
{
    Task<CheckIbanResponse> CheckIbanAsync(string iban);
    Task<GetTransferBankResponse> GetTransferBankAsync(GetTransferBankRequest request);
    Task<SaveTransferResponse> SaveTransferAsync(SaveTransferRequest request);
    Task UpdateTransferBankAsync(UpdateTransferBankRequest request);
    Task<TransactionExistsResponse> TransactionExistsAsync(Guid transactionSourceReferenceId);
}
