using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Application.Features.Reporting.Queries.GetProblemRecords;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByFile;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByNetwork;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryDaily;
using LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryOverall;
using LinkPara.Card.Application.Features.Reporting.Queries.GetTransactions;
using LinkPara.Card.Application.Features.Reporting.Queries.GetUnmatchedRecords;

namespace LinkPara.Card.Application.Commons.Interfaces.Reporting;

public interface IReportingService
{
    Task<GetTransactionsResponse> GetTransactionsAsync(GetTransactionsQuery query, CancellationToken ct = default);
    Task<GetTransactionsResponse> GetProblemRecordsAsync(GetProblemRecordsQuery query, CancellationToken ct = default);
    Task<GetTransactionsResponse> GetUnmatchedRecordsAsync(GetUnmatchedRecordsQuery query, CancellationToken ct = default);
    Task<GetSummaryDailyResponse> GetSummaryDailyAsync(GetSummaryDailyQuery query, CancellationToken ct = default);
    Task<GetSummaryByNetworkResponse> GetSummaryByNetworkAsync(GetSummaryByNetworkQuery query, CancellationToken ct = default);
    Task<GetSummaryByFileResponse> GetSummaryByFileAsync(GetSummaryByFileQuery query, CancellationToken ct = default);
    Task<GetSummaryOverallResponse> GetSummaryOverallAsync(CancellationToken ct = default);
}

