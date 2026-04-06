namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifEnrollmentRequest : VakifPaymentBase
{
    public Guid VerifyEnrollmentRequestId { get; set; }
    public string BrandName { get; set; }
    public string SuccessUrl { get; set; }
    public string FailureUrl { get; set; }
    public string OrderId { get; set; }
    public string AuthCode { get; set; }

    public Dictionary<string, string> BuildRequest()
    {
        var httpParameters = new Dictionary<string, string>
        {
            { "Pan", Pan },
            { "ExpiryDate", Expiry },
            { "PurchaseAmount", CurrencyAmount },
            { "Currency", CurrencyCode.ToString() },
            { "BrandName", BrandName },
            { "VerifyEnrollmentRequestId", Guid.NewGuid().ToString() },
            { "MerchantId", MerchantId },
            { "MerchantPassword", Password },
            { "MerchantType", MerchantType },
            { "SuccessUrl", SuccessUrl },
            { "FailureUrl", FailureUrl }
        };

        if (IsTopUpPayment != true)
        {
            httpParameters.Add("SubMerchantId", HostSubMerchantId);
        }

        if (NumberOfInstallments > 1)
            httpParameters.Add("InstallmentCount", NumberOfInstallments.ToString());

        return httpParameters;
    }
}
