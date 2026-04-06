using LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;
using LinkPara.PF.Application.Features.MerchantTransactions.Command;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IOrderNumberGeneratorService
{
    Task<OrderNumberResponse> GenerateAsync(GenerateOrderNumberCommand request);
    Task<string> GenerateForBankTransactionAsync(int bankCode, string merchantNumber);
    Task<string> GenerateForPhysicalPosTransactionAsync(int bankCode, string merchantNumber, string merchantName);
}
