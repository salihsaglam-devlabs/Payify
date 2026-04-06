using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;

public class IksTerminalHistoryDto
{
    public Guid Id { get; set; }
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
