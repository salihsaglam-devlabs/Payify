using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities;

public class CustomerWalletCard : AuditEntity
{
    public string BankingCustomerNo { get; set; }
    public string WalletNumber { get; set; }
    public string CardNumber { get; set; }
    public string ProductCode { get; set; }
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}
