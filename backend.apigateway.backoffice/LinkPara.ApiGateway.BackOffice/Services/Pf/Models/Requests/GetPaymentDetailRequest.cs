using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetPaymentDetailRequest : SearchQueryParams
{
    public string LinkCode { get; set; }
}
