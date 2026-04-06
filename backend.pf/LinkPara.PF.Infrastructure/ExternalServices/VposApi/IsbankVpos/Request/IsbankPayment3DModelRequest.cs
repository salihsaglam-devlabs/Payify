using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;

public class IsbankPayment3DModelRequest : IsbankPaymentRequestBase
{
    public string ThreeDSTransactionId { get; set; }
    public string BuildRequest()
    {
        var request = new
        {
            order_number = OrderNumber,
            merchant_number = MerchantNumber,
            three_ds_transaction_id = ThreeDSTransactionId,
            currency = Currency,
            amount = Amount,
            installment = Installment,
            use_point_in_sale = PointAmount > 0m,
            point_amount_to_use = PointAmount,
            point_type_to_use = "Maxipoint",
            payment_facilitator_id = new
            {
                sub_merchant_id = SubMerchantId,
                sub_merchant_mcc = SubMerchantMcc,
                sub_merchant_citizen_id = SubMerchantCitizenId,
                sub_merchant_city = SubMerchantCity,
                sub_merchant_postal_code = SubMerchantPostalCode,
                sub_merchant_url = SubMerchantUrl
            }
        };

        return JsonConvert.SerializeObject(request, Formatting.Indented);
    }
}