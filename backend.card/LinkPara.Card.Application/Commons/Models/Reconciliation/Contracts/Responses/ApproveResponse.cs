using System.ComponentModel.DataAnnotations;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;

public class ApproveResponse : ReconciliationResponseBase
{
    public Guid OperationId { get; set; }

    [MaxLength(100)]
    public string Result { get; set; }
}
