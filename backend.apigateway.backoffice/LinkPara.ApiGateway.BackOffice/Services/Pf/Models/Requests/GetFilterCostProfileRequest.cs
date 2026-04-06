using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterCostProfileRequest : SearchQueryParams
{
    public int? BankCode { get; set; }
    public PosType PosType { get; set; }
    public Guid? VposId { get; set; }
    public Guid? PhysicalPosId { get; set; }
    public ProfileStatus? ProfileStatus { get; set; }
    public ProfileSettlementMode? ProfileSettlementMode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
