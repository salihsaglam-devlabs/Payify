using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public interface IBillingHttpClient
{
    Task<BillInquiryResponseDto> InquireBillAsync(BillInquiryRequest request);
    Task<BillPaymentResponseDto> PayInquiredBillAsync(BillPaymentRequest request,Guid userId);
    Task<BillCancelResponseDto> CancelBillPaymentAsync(BillCancelRequest request);
    Task<BillPreviewResponseDto> BillPreviewAsync(BillPreviewRequest request, Guid userId);
 }