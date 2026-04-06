using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class WalletBlockageDto
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string WalletNumber { get; set; }
    public string AccountName { get; set; }
    public string WalletCurrencyCode { get; set; }
    public decimal CashBlockageAmount { get; set; }
    public decimal CreditBlockageAmount { get; set; }
    public WalletBlockageOperationType OperationType { get; set; }
    public WalletBlockageStatus BlockageStatus { get; set; }
    public string BlockageType { get; set; }
    public string BlockageDescription { get; set; }
    public DateTime BlockageStartDate { get; set; }
    public DateTime? BlockageEndDate { get; set; }

}
