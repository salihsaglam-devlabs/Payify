using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveCostProfileRequest
{
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PointCommission { get; set; }
    public decimal ServiceCommission { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public PosType PosType { get; set; }
    public Guid? VposId { get; set; }
    public Guid? PhysicalPosId { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
}
