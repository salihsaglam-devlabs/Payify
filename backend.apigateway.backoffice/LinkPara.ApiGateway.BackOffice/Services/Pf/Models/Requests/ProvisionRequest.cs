using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class ProvisionRequest
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public PaymentType PaymentType { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string OriginalOrderId { get; set; }
    public string LanguageCode { get; set; }
    public string MerchantCustomerName { get; set; }
    public string MerchantCustomerPhoneCode { get; set; }
    public string MerchantCustomerPhoneNumber { get; set; }
    public string Description { get; set; }
    public string CardHolderName { get; set; }
    public bool? IsOnUsPayment { get; set; }
    public string CallbackUrl { get; set; }
}

public class ProvisionMerchantRequest : ProvisionRequest
{
    private const string DefaultLanguageCode = "TR";

    public ProvisionMerchantRequest(ProvisionRequest request)
    {
        Amount = request.Amount;
        PointAmount = request.PointAmount;
        CardToken = request.CardToken;
        Currency = request.Currency;
        PaymentType = request.PaymentType;
        InstallmentCount = request.InstallmentCount;
        ThreeDSessionId = request.ThreeDSessionId;
        OriginalOrderId = request.OriginalOrderId;
        LanguageCode = request.LanguageCode ?? DefaultLanguageCode;
        MerchantCustomerName= request.MerchantCustomerName;
        MerchantCustomerPhoneCode = request.MerchantCustomerPhoneCode;
        MerchantCustomerPhoneNumber= request.MerchantCustomerPhoneNumber;
        Description = request.Description;
        CardHolderName = request.CardHolderName;
        IsOnUsPayment = request.IsOnUsPayment;
        CallbackUrl = request.CallbackUrl;
    }

    public Guid MerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
}
