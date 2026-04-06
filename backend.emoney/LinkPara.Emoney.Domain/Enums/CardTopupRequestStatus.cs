namespace LinkPara.Emoney.Application.Commons.Enums;

public enum CardTopupRequestStatus
{
    Pending,
    Completed,
    ProvisionTimeout,
    Reversed,
    Returned,
    Failed,
    WalletActionRequired,
    BankActionRequired
}