using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.PatchMerchantTransaction;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantTransactionService
{
    Task<UpdateMerchantTransactionRequest> PatchAsync(PatchMerchantTransactionCommand patchMerchantTransactionCommand);
    Task<List<MerchantTransactionDto>> GetMerchantTransactionsAsync(MerchantTransactionRequest request);
}