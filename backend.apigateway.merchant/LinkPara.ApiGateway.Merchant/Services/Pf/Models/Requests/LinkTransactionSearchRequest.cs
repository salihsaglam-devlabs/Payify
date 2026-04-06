using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class LinkTransactionSearchRequest
{
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public string OrderId { get; set; }
    public TransactionType? TransactionType { get; set; }
    public ChannelPaymentStatus? LinkPaymentStatus { get; set; }
    public bool? CommissionFromCustomer { get; set; }
}
