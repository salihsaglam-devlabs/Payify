using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterBankLimitRequest : SearchQueryParams
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public Guid? AcquireBankId { get; set; }
    public decimal? MonthlyLimitAmount { get; set; }
    public BankLimitType? BankLimitType { get; set; }
    public DateTime? LastValidDate { get; set; }
    public bool? IsExpired { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
