using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;

public class GetAllTerminalHistoryRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public Guid? VposId { get; set; }
    public string ReferenceCode { get; set; }
    public DateTime? QueryDateStart { get; set; }
    public DateTime? QueryDateEnd { get; set; }
    public TerminalRecordType? TerminalRecordType { get; set; }
}
