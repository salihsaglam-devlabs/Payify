using System;
using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class ManualReview
{
    public Guid OperationId { get; set; }
    public Guid FileLineId { get; set; }

    [MaxLength(200)]
    public string OperationCode { get; set; }
    public string OperationPayload { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<BranchOperation> ApproveBranchOperations { get; set; } = new();
    
    public List<BranchOperation> RejectBranchOperations { get; set; } = new();
    
    public DateTime? ExpiresAt { get; set; }
    
    public string ExpirationAction { get; set; }
    
    public string ExpirationFlowAction { get; set; }
    
    public string ApprovalMessage { get; set; }
    
    public string RejectionMessage { get; set; }
}
public class BranchOperation
{
    public string Code { get; set; }
    
    public string Payload { get; set; }
}