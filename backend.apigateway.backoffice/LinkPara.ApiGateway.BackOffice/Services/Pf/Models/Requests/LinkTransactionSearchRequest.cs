using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class LinkTransactionSearchRequest
{
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public string OrderId { get; set; }
    public LinkTransactionType? TransactionType { get; set; }
    public ChannelPaymentStatus? LinkPaymentStatus { get; set; }
    public bool? CommissionFromCustomer { get; set; }
}
