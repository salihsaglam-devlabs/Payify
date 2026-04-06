using LinkPara.Accounting.Domain.Entities;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

namespace LinkPara.Accounting.Application.Commons.Models;

public class ProcessInvoiceRequest
{
    public string CustomerCode { get; set; }
    public List<Payment> Payments { get; set; }
    public AccountingTransactionType TransactionType { get; set; }
    public CommissionType CommissionType { get; set; }
}

public enum CommissionType
{
    Sender,
    Receiver
}
