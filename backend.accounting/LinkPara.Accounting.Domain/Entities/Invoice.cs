using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Accounting.Domain.Entities;

public class Invoice : BaseEntity
{
    public string Code { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalBsmv { get; set; }
    public DateTime TransactionDate { get; set; }
    public AccountingTransactionType TransactionType { get; set; }
}
