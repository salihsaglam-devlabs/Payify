namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

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