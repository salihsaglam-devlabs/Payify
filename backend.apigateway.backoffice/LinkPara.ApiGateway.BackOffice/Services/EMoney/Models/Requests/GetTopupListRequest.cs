using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetTopupListRequest : SearchQueryParams
{
    public string Name { get; set; }
    public CardTopupRequestStatus? Status { get; set; }
    public string WalletNumber { get; set; }
    public string BinNumber { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PaymentProviderType? PaymentProviderType { get; set; }
    public CardType? CardType { get; set; }
}
