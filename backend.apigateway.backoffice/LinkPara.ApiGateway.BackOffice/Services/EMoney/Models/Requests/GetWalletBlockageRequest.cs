using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetWalletBlockageRequest
{
    public Guid? WalletId { get; set; }
    public string WalletNumber { get; set; }
    public string AccountName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public WalletBlockageStatus? BlockageStatus { get; set; }
    public string BlockageAdderUserName { get; set; }
}