namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;

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