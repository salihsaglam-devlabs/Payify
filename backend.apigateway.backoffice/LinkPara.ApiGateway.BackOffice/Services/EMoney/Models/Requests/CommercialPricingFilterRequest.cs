using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class CommercialPricingFilterRequest : SearchQueryParams
{
    public PricingCommercialType? PricingCommercialType { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime? ActivationDate { get; set; }
    public PricingCommercialStatus? PricingCommercialStatus { get; set; }
}