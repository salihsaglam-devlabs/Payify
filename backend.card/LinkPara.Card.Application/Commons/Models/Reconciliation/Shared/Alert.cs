using System;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class Alert
{
    public Guid Id { get; set; }

    public Guid FileLineId { get; set; }

    public Guid GroupId { get; set; }

    public Guid EvaluationId { get; set; }

    public Guid OperationId { get; set; }

    [MaxLength(20)]
    public string Severity { get; set; }

    [MaxLength(200)]
    public string AlertType { get; set; }

    [MaxLength(2000)]
    public string Message { get; set; }

    public DateTime CreatedAt { get; set; }
}

