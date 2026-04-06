using System.Text.Encodings.Web;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request
{
    public class OzanPaymentRequest : OzanPaymentBase
    {
        public string BuildRequest()
        {
            var requestObject = new
            {
                apiKey = ApiKey,
                providerKey = ProviderKey,
                amount = Amount,
                currency = Currency,
                number = Number,
                expiryMonth = ExpiryMonth,
                expiryYear = ExpiryYear,
                cvv = Cvv,
                referenceNo = ReferenceNo,
                is3d = Is3D,
                only3d = Only3D,
                isPreAuth = IsPreAuth,
                installment = Installment,
                billingFirstName = BillingFirstName,
                billingLastName = BillingLastName,
                email = Email,
                billingCompany = BillingCompany,
                billingPhone = BillingPhone,
                billingAddress1 = BillingAddress1,
                billingCountry = BillingCountry,
                billingCity = BillingCity,
                billingPostcode = BillingPostCode,
                basketItems = BasketItems,
                returnUrl = ReturnUrl,
                customerIp = CustomerIp,
                customerUserAgent = CustomerUserAgent,
                browserInfo = BrowserInfo
            };

            return JsonSerializer.Serialize(requestObject, new JsonSerializerOptions { 
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}
