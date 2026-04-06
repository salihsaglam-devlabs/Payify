using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class UpdatePhysicalPosRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public VposType VposType { get; set; }
    public string PfMainMerchantId { get; set; }
}
