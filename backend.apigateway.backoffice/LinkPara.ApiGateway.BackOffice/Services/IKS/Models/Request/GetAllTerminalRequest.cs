using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;

public class GetAllTerminalRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public Guid? VposId { get; set; }
    public string ReferenceCode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
