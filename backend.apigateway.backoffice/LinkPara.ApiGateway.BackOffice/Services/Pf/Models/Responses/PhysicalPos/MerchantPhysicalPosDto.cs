using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class MerchantPhysicalPosDto
{
    public Guid Id { get; set; }
    public Guid MerchantPhysicalDeviceId { get; set; }
    public Guid PhysicalPosId { get; set; }
    public string PosMerchantId { get; set; }
    public string PosTerminalId { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string BkmReferenceNumber { get; set; }
    public PhysicalPosDto PhysicalPos { get; set; }
}
