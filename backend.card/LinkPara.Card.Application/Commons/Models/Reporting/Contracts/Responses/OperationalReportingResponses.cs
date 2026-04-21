using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Dtos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;

public class GetActionRadarResponse : ReconciliationResponseBase
{
    public PaginatedList<ActionRadarDto> Data { get; set; }
}

public class GetUnhealthyFilesResponse : ReconciliationResponseBase
{
    public PaginatedList<UnhealthyFileDto> Data { get; set; }
}

public class GetStuckPipelineItemsResponse : ReconciliationResponseBase
{
    public PaginatedList<StuckPipelineItemDto> Data { get; set; }
}

public class GetReconFailureCategorizationResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconFailureCategorizationDto> Data { get; set; }
}

public class GetManualReviewPressureResponse : ReconciliationResponseBase
{
    public PaginatedList<ManualReviewPressureDto> Data { get; set; }
}

public class GetAlertDeliveryHealthResponse : ReconciliationResponseBase
{
    public PaginatedList<AlertDeliveryHealthDto> Data { get; set; }
}

public class GetUnmatchedFinancialExposureResponse : ReconciliationResponseBase
{
    public PaginatedList<UnmatchedFinancialExposureDto> Data { get; set; }
}

public class GetCardClearingImbalanceResponse : ReconciliationResponseBase
{
    public PaginatedList<CardClearingImbalanceDto> Data { get; set; }
}

public class GetReconciliationQualityScoreResponse : ReconciliationResponseBase
{
    public PaginatedList<ReconciliationQualityScoreDto> Data { get; set; }
}

public class GetMisleadingSuccessCasesResponse : ReconciliationResponseBase
{
    public PaginatedList<MisleadingSuccessCaseDto> Data { get; set; }
}

public class GetArchivePipelineHealthResponse : ReconciliationResponseBase
{
    public PaginatedList<ArchivePipelineHealthDto> Data { get; set; }
}

public class GetReportingDocumentationResponse : ReconciliationResponseBase
{
    public PaginatedList<ReportingDocumentationDto> Data { get; set; }
}

