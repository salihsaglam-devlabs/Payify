using System.ComponentModel.DataAnnotations;
using LinkPara.Card.Domain.Enums.Reconciliation;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class GetAlertsRequest
{
    public DateOnly? Date { get; set; }
    
    public AlertStatus? AlertStatus { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 1000)]
    public int PageSize { get; set; } = 50;
}
