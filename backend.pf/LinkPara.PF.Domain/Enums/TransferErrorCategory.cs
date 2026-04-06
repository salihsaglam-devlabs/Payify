namespace LinkPara.PF.Domain.Enums;

public enum TransferErrorCategory
{
    MerchantIsNotActiveOrExists,
    MoreThanOneProfileInUse,
    MoreThanOneProfileItemInUse,
    PricingProfileNotFound,
    PricingProfileItemNotFound,
    TransferSystemError,
    CouldNotCalculateDueDate,
    DuplicateTransaction,
    NegativePfNetCommissionAmount
}