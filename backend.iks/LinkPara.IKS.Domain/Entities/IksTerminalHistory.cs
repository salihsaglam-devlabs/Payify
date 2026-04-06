using LinkPara.IKS.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.IKS.Domain.Entities;

public class IksTerminalHistory : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Guid VposId { get; set; }
    public Guid PhysicalPosId { get; set; }
    public string ReferenceCode { get; set; }
    public string NewData { get; set; }
    public string OldData { get; set; }
    public string ChangedField { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseCodeExplanation { get; set; }
    public DateTime QueryDate { get; set; }
    public TerminalRecordType TerminalRecordType { get; set; }
}
