using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class InquireResponse : ResponseBase
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