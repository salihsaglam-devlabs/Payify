namespace LinkPara.Emoney.Domain.Enums;

public enum TierPermissionType
{
    P2PTransfer,
    WithdrawToOtherIban,
    SaveOtherIban,
    WithdrawToOwnIban,
    SaveOwnIban,
    DepositFromOwnIban,
    DepositFromOtherIban,
    OnUsPayment
}