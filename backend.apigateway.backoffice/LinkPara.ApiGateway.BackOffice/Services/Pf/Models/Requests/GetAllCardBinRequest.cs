using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllCardBinRequest : SearchQueryParams
{
    public CardBrand? CardBrand { get; set; }
    public CardType? CardType { get; set; }
    public CardNetwork? CardNetwork { get; set; }
    public CardSubType? CardSubType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
