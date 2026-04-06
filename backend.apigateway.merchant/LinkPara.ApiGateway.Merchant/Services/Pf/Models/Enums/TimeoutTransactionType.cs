namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

public enum TimeoutTransactionType
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
