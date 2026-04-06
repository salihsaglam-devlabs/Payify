namespace LinkPara.HttpProviders.Fraud.Models.Enums
{
    public enum Response
    {
        None,
        Successful,
        Unsuccessful,
        ThreeDError,
        CVVError,
        ExpiryDateError,
        CardNumberError,
        InsufficientFunds
    }
}
