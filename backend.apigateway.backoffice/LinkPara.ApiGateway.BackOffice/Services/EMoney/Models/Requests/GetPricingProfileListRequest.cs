using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetPricingProfileListRequest : SearchQueryParams
{
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TransferType? TransferType { get; set; }
    public PricingProfileStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CardType? CardType { get; set; }
}
