namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

public enum PostingTransferErrorCategory
{
    MerchantIsNotActiveOrExists,
    MoreThanOneProfileInUse,
    MoreThanOneProfileItemInUse,
    PricingProfileNotFound,
    PricingProfileItemNotFound,
    TransferSystemError
}