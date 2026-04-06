using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class GetAllPhysicalPosRequest : SearchQueryParams
{
    public int? BankCode { get; set; }
    public VposStatus? VposStatus { get; set; }
    public VposType? VposType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
