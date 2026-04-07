using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ExecuteOptions
{
    [Range(1, 5000)]
    public int MaxEvaluations { get; set; } = 5000;

    [Range(1, 3600)]
    public int LeaseSeconds { get; set; } = 30;
}
