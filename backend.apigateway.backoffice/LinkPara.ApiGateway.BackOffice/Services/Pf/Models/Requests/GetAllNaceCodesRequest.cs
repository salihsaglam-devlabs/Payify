using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllNaceCodesRequest : SearchQueryParams
{
    public string SectorCode { get; set; }
    public string ProfessionCode { get; set; }
    public string Code { get; set; } 
}