using System;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class OperationExecutionResult
{
    [Required]
    public Guid OperationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Status { get; set; }

    [MaxLength(2000)]
    public string Message { get; set; }
}
