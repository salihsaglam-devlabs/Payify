namespace LinkPara.HttpProviders.PF.Models.Enums;

public enum DeductionType
{
    Chargeback,
    RejectedChargeback,
    Suspicious,
    RejectedSuspicious,
    Due,
    ExcessReturn,
    SubMerchantDeduction,
    SubMerchantChargebackCommission,
    SubMerchantRejectedChargebackCommission,
    SubMerchantSuspiciousCommission,
    SubMerchantRejectedSuspiciousCommission,
}