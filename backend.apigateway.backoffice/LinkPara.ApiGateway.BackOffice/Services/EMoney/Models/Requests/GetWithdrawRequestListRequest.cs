using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetWithdrawRequestListRequest : SearchQueryParams
{
    public WithdrawStatus? WithdrawStatus { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public TransferType? TransferType { get; set; }
}