using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public interface IBillingHttpClient
{
    Task<PaginatedList<BillTransactionDto>> GetBillTransactionsAsync(BillTransactionFilterRequest request);
    Task<BillTransactionDto> GetBillInfoByConversationIdAsync(string conversationId);
    Task<BillCancelResponseDto> CancelBillPaymentAsync(BillCancelRequest request);
    Task<BillCancelAccountingResponseDto> CancelBillAccountingAsync(BillCancelAccountingRequest request);
}