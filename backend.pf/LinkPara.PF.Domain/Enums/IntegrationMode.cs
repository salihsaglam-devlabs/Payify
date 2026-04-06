namespace LinkPara.PF.Domain.Enums;

[Flags]
public enum IntegrationMode
{
    Unknown = 0,
    Api = 1,
    Hpp = 2,
    ManuelPaymentPage = 4,
    LinkPaymentPage = 8,
    OnUs = 16
}