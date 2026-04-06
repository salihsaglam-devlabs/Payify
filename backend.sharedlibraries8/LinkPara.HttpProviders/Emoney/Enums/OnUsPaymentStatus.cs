namespace LinkPara.HttpProviders.Emoney.Enums
{
    public enum OnUsPaymentStatus
    {
        Pending,
        Success,
        Failed,
        Expired,
        Rejected,
        Suspecious,
        Chargeback, 
        Returned,
        PartiallyReturned,
        Canceled
    }
}
