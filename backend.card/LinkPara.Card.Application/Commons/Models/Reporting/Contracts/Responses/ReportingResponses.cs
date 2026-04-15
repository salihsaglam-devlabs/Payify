using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;

public class GetTransactionsResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconciliationTransactionDto> Data { get; set; }
}

public class GetSummaryDailyResponse : ReconciliationResponseBase
{
    public List<ReconciliationSummaryDailyDto> Data { get; set; } = new();
}

public class GetSummaryByNetworkResponse : ReconciliationResponseBase
{
    public List<ReconciliationSummaryByNetworkDto> Data { get; set; } = new();
}

public class GetSummaryByFileResponse : ReconciliationResponseBase
{
    public List<ReconciliationSummaryByFileDto> Data { get; set; } = new();
}

public class GetSummaryOverallResponse : ReconciliationResponseBase
{
    public ReconciliationSummaryOverallDto Data { get; set; }
}

