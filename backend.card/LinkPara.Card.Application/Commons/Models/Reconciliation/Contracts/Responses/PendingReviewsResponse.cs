using System.ComponentModel.DataAnnotations;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;

public class PendingReviewsResponse : ReconciliationResponseBase
{
    [Required]
    public PagedResult<ManualReview> Page { get; set; } = new();
}
