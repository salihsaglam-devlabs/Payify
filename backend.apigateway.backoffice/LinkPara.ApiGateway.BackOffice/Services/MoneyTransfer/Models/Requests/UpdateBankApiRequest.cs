using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class UpdateBankApiRequest
{
    public Guid Id { get; set; }
    public string ApiName { get; set; }
    public string CurrencyCode { get; set; }
    public string EftStartTime { get; set; }
    public string EftEndTime { get; set; }
    public bool DefaultEftBank { get; set; }
    public bool DefaultFastBank { get; set; }
    public bool FastAvailable { get; set; }
    public bool DefaultCheckIbanBank { get; set; }
    public bool IbanCheckAvailable { get; set; }
    public int BankCode { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool ReconciliationCompareCharge { get; set; }
    public bool ProcessReturnedEft { get; set; }
    public bool ProcessReverseTransfer { get; set; }
    public bool DefaultPfTransferBank { get; set; }
}
