using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.Posting
{
    public class PatchPostingBalanceRequest : IMapFrom<PostingBalance>
    {
        public int MoneyTransferBankCode { get; set; }
        public string MoneyTransferBankName { get; set; }
    }
}
