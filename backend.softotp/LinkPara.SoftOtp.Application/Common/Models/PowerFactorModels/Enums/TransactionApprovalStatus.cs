namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Enums;

public enum TransactionApprovalStatus
{
    Rejected,
    Approved,    
    WaitingApproval,    
    ErrorOccurred,    
    Expired,
    Fraud,
    Cancelled,
    WrongPIN,
    Unknown
}