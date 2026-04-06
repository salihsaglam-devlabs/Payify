using LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Accounting.Models.Requests;

public class SavePaymentRequest
{
    public string ReferenceId { get; set; }
    public AccountingOperationType OperationType { get; set; }
    public bool HasCommission { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public int BankCode { get; set; }
    public AccountingTransactionType AccountingTransactionType { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
}
