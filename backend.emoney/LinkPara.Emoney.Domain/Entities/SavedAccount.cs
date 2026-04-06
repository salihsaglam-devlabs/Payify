using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public abstract class SavedAccount : AuditEntity
{
    public Guid UserId { get; set; }
    public string Tag { get; set; }
    public string Type { get; set; }
    public string ReceiverName { get; set; }

}

public class SavedWalletAccount : SavedAccount
{
    public string WalletNumber { get; set; }
    public Guid WalletOwnerAccountId { get; set; }
}

public class SavedBankAccount : SavedAccount
{
    public string Iban { get; set; }
    public Guid BankId { get; set; }
    public virtual Bank Bank { get; set; }
}

