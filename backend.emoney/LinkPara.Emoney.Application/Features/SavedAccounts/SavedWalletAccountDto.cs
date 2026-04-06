using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.SavedAccounts;

public class SavedWalletAccountDto : IMapFrom<SavedWalletAccount>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Tag { get; set; }
    public string WalletNumber { get; set; }
    public Guid WalletOwnerAccountId { get; set; }
    public string WalletOwnerName { get; set; }
    public string ReceiverName { get; set; }
}