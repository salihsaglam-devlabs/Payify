using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class RejectRequest
{
    [Required]
    public Guid OperationId { get; set; }

    public Guid? ReviewerId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Comment { get; set; }
}
