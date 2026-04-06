using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Utility;

namespace LinkPara.HttpProviders.Fraud.Models;

public class TransactionResponse : BaseResponse
{
    public string TransactionId { get; set; }
    public RiskLevel RiskLevel { get; set; }
}
