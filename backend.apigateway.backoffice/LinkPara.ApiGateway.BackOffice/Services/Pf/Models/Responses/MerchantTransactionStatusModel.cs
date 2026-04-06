using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantTransactionStatusModel
{
    public PfTransactionStatus TransactionStatus { get; set; }
    public int Count { get; set; }
    public double Percent { get; set; }
}
