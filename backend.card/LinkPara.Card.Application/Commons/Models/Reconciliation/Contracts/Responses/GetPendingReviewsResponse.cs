using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;

public class GetPendingReviewsResponse : ReconciliationResponseBase
{
    public PaginatedList<ManualReview> Data { get; set; }
}

