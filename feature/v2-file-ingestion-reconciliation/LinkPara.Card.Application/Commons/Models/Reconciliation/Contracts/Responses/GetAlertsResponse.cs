using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class GetAlertsResponse : ReconciliationResponseBase
{
    [Required]
    public PagedResult<Alert> Page { get; set; } = new();
}
