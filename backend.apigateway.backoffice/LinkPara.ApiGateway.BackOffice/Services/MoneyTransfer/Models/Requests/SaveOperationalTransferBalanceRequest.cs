namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class SaveOperationalTransferBalanceRequest
{
    public int BankCode { get; set; }
    public decimal MinimumAmount { get; set; }
    public decimal TransferBalance { get; set; }
    public decimal TransferBalanceForHoliday { get; set; }
}
