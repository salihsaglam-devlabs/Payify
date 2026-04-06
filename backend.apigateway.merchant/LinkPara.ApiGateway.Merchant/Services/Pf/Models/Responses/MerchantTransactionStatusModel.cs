using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantTransactionStatusModel
{
    public PfTransactionStatus TransactionStatus { get; set; }
    public int Count { get; set; }
    public double Percent { get; set; }
}
