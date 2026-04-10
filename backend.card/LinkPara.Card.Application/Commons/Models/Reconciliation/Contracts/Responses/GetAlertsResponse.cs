using System.ComponentModel.DataAnnotations;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;

public class GetAlertsResponse : ReconciliationResponseBase
{
    [Required]
    public PagedResult<Alert> Page { get; set; } = new();
}
