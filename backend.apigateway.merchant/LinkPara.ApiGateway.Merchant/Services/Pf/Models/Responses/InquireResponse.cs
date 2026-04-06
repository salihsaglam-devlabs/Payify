using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class InquireResponse : ResponseModel
{
    public string PaymentConversationId { get; set; }
    public string OrderId { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string Currency { get; set; }
    public int InstallmentCount { get; set; }
    public string BinNumber { get; set; }
    public string CardNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public CardType CardType { get; set; }
    public int IssuerBankCode { get; set; }
    public bool Is3ds { get; set; }
    public List<ProvisionModel> ProvisionList { get; set; }
    public ReturnStatus ReturnStatus { get; set; }
}

public class ProvisionModel
{
    public decimal Amount { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionDate { get; set; }
}