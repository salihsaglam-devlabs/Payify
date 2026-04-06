using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class Provision : AuditEntity
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public ProvisionSource ProvisionSource { get; set; }
    public ProvisionStatus ProvisionStatus { get; set; }
    public string Description { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public bool IsReturn { get; set; }
    public DateTime ReturnDate { get; set; }
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    public Guid? PartnerId { get; set; }
    public Partner Partner { get; set; }
    public string ProvisionReference { get; set; }
    public Guid? PaymentProvisionId { get; set; }
    public Provision PaymentProvision { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
}