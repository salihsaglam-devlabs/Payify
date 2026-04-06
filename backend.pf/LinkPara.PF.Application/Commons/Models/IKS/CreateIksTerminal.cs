using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.IKS;

public class CreateIksTerminal
{
    public Guid MerchantId { get; set; }
    public PosType PosType { get; set; }
}
