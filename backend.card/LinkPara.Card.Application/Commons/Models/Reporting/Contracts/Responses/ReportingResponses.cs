using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;

public class GetIngestionFileOverviewResponse : ReconciliationResponseBase
{
    public PaginatedList<IngestionFileOverviewDto> Data { get; set; }
}

public class GetIngestionFileQualityResponse : ReconciliationResponseBase
{
    public PaginatedList<IngestionFileQualityDto> Data { get; set; }
}

public class GetIngestionDailySummaryResponse : ReconciliationResponseBase
{
    public List<IngestionDailySummaryDto> Data { get; set; } = new();
}

public class GetIngestionNetworkMatrixResponse : ReconciliationResponseBase
{
    public List<IngestionNetworkMatrixDto> Data { get; set; } = new();
}

public class GetIngestionExceptionHotspotsResponse : ReconciliationResponseBase
{
    public PaginatedList<IngestionExceptionHotspotDto> Data { get; set; }
}

public class GetReconDailyOverviewResponse : ReconciliationResponseBase
{
    public List<ReconDailyOverviewDto> Data { get; set; } = new();
}

public class GetReconOpenItemsResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconOpenItemDto> Data { get; set; }
}

public class GetReconOpenItemAgingResponse : ReconciliationResponseBase
{
    public List<ReconOpenItemAgingDto> Data { get; set; } = new();
}

public class GetReconManualReviewQueueResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconManualReviewQueueDto> Data { get; set; }
}

public class GetReconAlertSummaryResponse : ReconciliationResponseBase
{
    public List<ReconAlertSummaryDto> Data { get; set; } = new();
}

public class GetReconCardContentDailyResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconCardContentDailyDto> Data { get; set; }
}

public class GetReconClearingContentDailyResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconClearingContentDailyDto> Data { get; set; }
}

public class GetReconContentDailyResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconContentDailyDto> Data { get; set; }
}

public class GetReconClearingControlStatAnalysisResponse : ReconciliationResponseBase
{
    public List<ReconClearingControlStatAnalysisDto> Data { get; set; } = new();
}

public class GetReconFinancialSummaryResponse : ReconciliationResponseBase
{
    public List<ReconFinancialSummaryDto> Data { get; set; } = new();
}

public class GetReconResponseStatusAnalysisResponse : ReconciliationResponseBase
{
    public List<ReconResponseStatusAnalysisDto> Data { get; set; } = new();
}

public class GetArchiveRunOverviewResponse : ReconciliationResponseBase
{
    public PaginatedList<ArchiveRunOverviewDto> Data { get; set; }
}

public class GetArchiveEligibilityResponse : ReconciliationResponseBase
{
    public PaginatedList<ArchiveEligibilityDto> Data { get; set; }
}

public class GetArchiveBacklogTrendResponse : ReconciliationResponseBase
{
    public List<ArchiveBacklogTrendDto> Data { get; set; } = new();
}

public class GetArchiveRetentionSnapshotResponse : ReconciliationResponseBase
{
    public ArchiveRetentionSnapshotDto Data { get; set; }
}

public class GetFileReconSummaryResponse : ReconciliationResponseBase
{
    public PaginatedList<FileReconSummaryDto> Data { get; set; }
}

public class GetReconMatchRateTrendResponse : ReconciliationResponseBase
{
    public List<ReconMatchRateTrendDto> Data { get; set; } = new();
}

public class GetReconGapAnalysisResponse : ReconciliationResponseBase
{
    public List<ReconGapAnalysisDto> Data { get; set; } = new();
}

public class GetUnmatchedTransactionAgingResponse : ReconciliationResponseBase
{
    public List<UnmatchedTransactionAgingDto> Data { get; set; } = new();
}

public class GetNetworkReconScorecardResponse : ReconciliationResponseBase
{
    public List<NetworkReconScorecardDto> Data { get; set; } = new();
}

public class GetCardClearingCorrelationResponse : ReconciliationResponseBase
{
    public PaginatedList<CardClearingCorrelationDto> Data { get; set; }
}
