using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllPostingPfProfitsRequest : SearchQueryParams
{
    public DateTime? PaymentDate { get; set; }
}