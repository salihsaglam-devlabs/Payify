using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllMerchantDueRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public Guid? DueProfileId { get; set; }
    public DueType? DueType { get; set; }
    public DateTime? LastExecutionDateStart { get; set; }
    public DateTime? LastExecutionDateEnd { get; set; }
    public int? ExecutionCountBigger { get; set; }
    public int? ExecutionCountLower { get; set; }
    public string Title { get; set; }
    public decimal? AmountBiggerThan { get; set; }
    public decimal? AmountSmallerThan { get; set; }
    public int? Currency { get; set; }
    public TimeInterval? OccurenceInterval { get; set; }
    public bool? IsDefault { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}