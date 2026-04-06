using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class OperationalTransferBalanceDto
{
    public Guid Id { get; set; }
    public int MainBankCode { get; set; }
    public int BankCode { get; set; }
    public decimal MinimumAmount { get; set; }
    public string IbanNumber { get; set; }
    public decimal TransferBalance { get; set; }
    public decimal TransferBalanceForHoliday { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
