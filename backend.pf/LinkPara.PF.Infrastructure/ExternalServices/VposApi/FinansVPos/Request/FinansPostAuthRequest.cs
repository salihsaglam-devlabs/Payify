using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansPostAuthRequest : FinansPaymentBase
{
    public string BuildRequest()
    {
        CardAcceptorStreet = CardAcceptorStreet.Substring(0, Math.Min(CardAcceptorStreet.Length, 32));
        CardAcceptorName = CardAcceptorName.Substring(0, Math.Min(CardAcceptorName.Length, 30));
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone =\"yes\" ?>");
        requestXml.AppendLine("<PayForRequest>");
        requestXml.AppendLine($"<MbrId>{MbrId.Trim()}</MbrId>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<UserCode>{UserCode.Trim()}</UserCode>");
        requestXml.AppendLine($"<UserPass>{UserPass.Trim()}</UserPass>");
        requestXml.AppendLine($"<OrderId>{OrderId.Trim()}</OrderId>");
        requestXml.AppendLine($"<Lang>{LanguageCode.Trim()}</Lang>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine($"<SecureType>{SecureType.Trim()}</SecureType>");
        requestXml.AppendLine($"<PurchAmount>{PurchAmount}</PurchAmount>");
        requestXml.AppendLine($"<TxnType>{TxnType.Trim()}</TxnType>");
        requestXml.AppendLine($"<PaymentFacilitatorId>{PaymentFacilitatorId.Trim()}</PaymentFacilitatorId>");
        requestXml.AppendLine($"<SubmerchantId>{SubmerchantId.Trim()}</SubmerchantId>");
        requestXml.AppendLine($"<SubmerchantMcc>{SubmerchantMcc.Trim()}</SubmerchantMcc>");
        requestXml.AppendLine($"<CardAcceptorName>{CardAcceptorName.Trim()}</CardAcceptorName>");
        requestXml.AppendLine($"<CardAcceptorStreet>{CardAcceptorStreet.Trim()}</CardAcceptorStreet>");
        requestXml.AppendLine($"<CardAcceptorCity>{CardAcceptorCity.Trim()}</CardAcceptorCity>");
        requestXml.AppendLine($"<CardAcceptorPostalCode>{CardAcceptorPostalCode.Trim()}</CardAcceptorPostalCode>");
        requestXml.AppendLine($"<CardAcceptorState>{CardAcceptorState.Trim()}</CardAcceptorState>");
        requestXml.AppendLine($"<CardAcceptorCountry>{CardAcceptorCountry.Trim()}</CardAcceptorCountry>");
        requestXml.AppendLine("</PayForRequest>");
        return requestXml.ToString();
    }
}
