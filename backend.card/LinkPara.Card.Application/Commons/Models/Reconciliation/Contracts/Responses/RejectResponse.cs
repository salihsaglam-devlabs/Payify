using System;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class RejectResponse : ReconciliationResponseBase
{
    public Guid OperationId { get; set; }

    [MaxLength(100)]
    public string Result { get; set; }
}
