using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Fraud.Models;

public class FraudCheckRequest
{
    public string CommandName { get; set; }
    public string CommandJson { get; set; }
    public string Module { get; set; }
    public string UserId { get; set; }
    public string ClientIpAddress { get; set; }
    public AccountKycLevel AccountKycLevel { get; set; }
    public FraudTransactionDetail ExecuteTransaction { get; set; }
}
