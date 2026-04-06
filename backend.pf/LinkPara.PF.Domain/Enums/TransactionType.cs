namespace LinkPara.PF.Domain.Enums;

public enum TransactionType
{
    Auth,
    PreAuth,
    Return,
    PostAuth,
    Reverse,
    Chargeback,
    Suspicious,
    RejectedChargeback,
    RejectedSuspicious,
    Due,
    ExcessReturn,
    ParentMerchantCommission,
    SubMerchantDeduction
}
