using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class PhysicalPosResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
}
