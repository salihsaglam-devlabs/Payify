namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;

public enum BillTransactionStatus
{
    New,
    Paid,
    Cancelled,
    Error,
    Poison,
    Timeout,
    ProvisionError
}