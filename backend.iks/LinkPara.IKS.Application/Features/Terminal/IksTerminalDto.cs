using LinkPara.IKS.Application.Commons.Mappings;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Domain.Enums;

namespace LinkPara.IKS.Application.Features.Terminal;

public class IksTerminalDto : IMapFrom<IksTerminal>
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid VposId { get; set; }
    public Guid PhysicalPosId { get; set; }
    public string ReferenceCode { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string TerminalId { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseCodeExplanation { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreatedNameBy { get; set; }
}
