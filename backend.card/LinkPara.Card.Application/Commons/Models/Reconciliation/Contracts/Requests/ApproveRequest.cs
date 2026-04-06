using System;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ApproveRequest
{
    [Required]
    public Guid OperationId { get; set; }

    public Guid? ReviewerId { get; set; }

    [MaxLength(2000)]
    public string Comment { get; set; }
}
