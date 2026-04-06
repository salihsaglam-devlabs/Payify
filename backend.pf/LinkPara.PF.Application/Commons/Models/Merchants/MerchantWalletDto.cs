using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantWalletDto: IMapFrom<MerchantWallet>
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
    public Guid MerchantId { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
