
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Accounting.Domain.Entities;

public class BankAccount : AuditEntity, ITrackChange
{
    public string BankName { get; set; }
    public int BankCode { get; set; }
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public AccountingTransactionType AccountingTransactionType { get; set; }
    public string AccountTag { get; set; }
}
