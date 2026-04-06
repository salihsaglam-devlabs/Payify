using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class GetBulkTransfersRequest : SearchQueryParams
{
    public BulkTransferStatus? BulkTransferStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
