using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class InquireResponseModel : ResponseModel
{
    public string PaymentConversationId { get; set; }
    public string OrderId { get; set; }
    public Enums.TransactionType TransactionType { get; set; }
    public PfTransactionStatus TransactionStatus { get; set; }
}