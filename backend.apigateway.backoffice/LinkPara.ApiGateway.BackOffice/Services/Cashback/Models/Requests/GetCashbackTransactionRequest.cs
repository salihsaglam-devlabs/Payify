using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;

public class GetCashbackTransactionRequest : SearchQueryParams
{
    public DateTime? EntitlementStartDate { get; set; }
    public DateTime? EntitlementEndDate { get; set; }
    public string WalletNumber { get; set; }
    public string Name { get; set; }
    public CashbackPaymentStatus? PaymentStatus { get; set; }
    public CashbackProcessStatus? ProcessStatus { get; set; }
    public RecordStatus? RuleStatus { get; set; }
    public decimal? MinTransactionAmount { get; set; }
    public decimal? MaxTransactionAmount { get; set; }
    public decimal? MinCashbackAmount { get; set; }
    public decimal? MaxCashbackAmount { get; set; }
    public string KycType { get; set; }
    public CashbackProcessType? ProcessType { get; set; }
    public Guid? RuleId { get; set; }
}
