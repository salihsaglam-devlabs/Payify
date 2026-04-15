namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class ApproveRequest
{
    public Guid OperationId { get; set; }

    public Guid? ReviewerId { get; set; }

    public string Comment { get; set; }
}
