using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterVposRequest : SearchQueryParams
{
    public int? BankCode { get; set; }
    public VposStatus? VposStatus { get; set; }
    public VposType? VposType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public bool? IsInsuranceVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public bool? IsOnUsVpos { get; set; }
}
