using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class AddWalletBlockageRequest
{
    public string WalletNumber { get; set; }
    public decimal CashBlockageAmount { get; set; }
    public decimal CreditBlockageAmount { get; set; }
    public WalletBlockageOperationType OperationType { get; set; }
    public string BlockageType { get; set; }
    public string BlockageDescription { get; set; }
    public DateTime BlockageStartDate { get; set; }
    public DateTime? BlockageEndDate { get; set; }
}