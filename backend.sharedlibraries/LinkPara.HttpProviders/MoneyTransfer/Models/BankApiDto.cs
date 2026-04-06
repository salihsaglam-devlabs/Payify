using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;

public class BankApiDto
{
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }

    public Guid Id { get; set; }
    public string ApiName { get; set; }
    public string CurrencyCode { get; set; }
    public TimeSpan EftStartTime { get; set; }
    public TimeSpan EftEndTime { get; set; }
    public bool DefaultEftBank { get; set; }
    public bool DefaultCheckIbanBank { get; set; }
    public bool IbanCheckAvailable { get; set; }
    public bool DefaultFastBank { get; set; }
    public bool FastAvailable { get; set; }
    public int BankCode { get; set; }
    public bool ReconciliationCompareCharge { get; set; }
    public bool ProcessReturnedEft { get; set; }
    public bool ProcessReverseTransfer { get; set; }
}
