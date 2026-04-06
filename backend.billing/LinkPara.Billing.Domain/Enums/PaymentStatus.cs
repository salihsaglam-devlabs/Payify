namespace LinkPara.Billing.Domain.Enums;

public enum PaymentStatus
{
    ExistBothSides,
    MissingTransaction,
    MissingVendorTransaction,
    CancelledTransaction
}