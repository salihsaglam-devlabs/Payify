using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class SavePhysicalPosRequest
{
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public VposType VposType { get; set; }
    public string PfMainMerchantId { get; set; }
}
