using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class PatchOperationalTransferBalanceRequest
{
    public decimal MinimumAmount { get; set; }
    public decimal TransferBalance { get; set; }
    public decimal TransferBalanceForHoliday { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
