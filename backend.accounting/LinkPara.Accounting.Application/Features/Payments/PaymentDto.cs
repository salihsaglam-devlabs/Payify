using LinkPara.Accounting.Application.Commons.Mappings;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

namespace LinkPara.Accounting.Application.Features.Payments;

public class PaymentDto : IMapFrom<Payment>
{
    public Guid Id { get; set; }
    public string ReferenceId { get; set; }
    public OperationType OperationType { get; set; }
    public bool HasCommission { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsSuccess { get; set; }
    public string ResultMessage { get; set; }
    public int BankCode { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
    public AccountingTransactionType AccountingTransactionType { get; set; }
    public string SourceFullName { get; set; }
    public string DestinationFullName { get; set; }
}
