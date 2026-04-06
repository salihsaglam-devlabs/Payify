namespace LinkPara.ApiGateway.Services.Emoney.Models.Enums;

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