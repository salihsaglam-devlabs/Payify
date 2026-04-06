using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansPaymentNonSecureRequest : FinansPaymentBase
{
    public string BuildRequest()
    {
        CardAcceptorStreet = CardAcceptorStreet.Substring(0, Math.Min(CardAcceptorStreet.Length, 32));
        CardAcceptorName = CardAcceptorName.Substring(0, Math.Min(CardAcceptorName.Length, 30));
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        requestXml.AppendLine("<PayForRequest>");
        requestXml.AppendLine($"<MbrId>{MbrId.Trim()}</MbrId>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<UserCode>{UserCode.Trim()}</UserCode>");
        requestXml.AppendLine($"<UserPass>{UserPass.Trim()}</UserPass>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<SecureType>{SecureType.Trim()}</SecureType>");
        requestXml.AppendLine($"<TxnType>{TxnType.Trim()}</TxnType>");
        requestXml.AppendLine($"<PurchAmount>{PurchAmount}</PurchAmount>");
        requestXml.AppendLine($"<BonusAmount>{BonusAmount}</BonusAmount>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine($"<Pan>{Pan.Trim()}</Pan>");
        requestXml.AppendLine($"<Expiry>{Expiry}</Expiry>");
        requestXml.AppendLine($"<Cvv2>{Cvv2.Trim()}</Cvv2>");
        requestXml.AppendLine($"<MOTO>{MOTO.Trim()}</MOTO>");
        requestXml.AppendLine($"<Lang>{LanguageCode.Trim()}</Lang>");
        requestXml.AppendLine($"<InstallmentCount>{InstallmentCount}</InstallmentCount>");
        requestXml.AppendLine($"<PaymentFacilitatorId>{PaymentFacilitatorId.Trim()}</PaymentFacilitatorId>");
        requestXml.AppendLine($"<SubmerchantCode>{SubmerchantId.Trim()}</SubmerchantCode>");            
        requestXml.AppendLine($"<SubmerchantMCC>{SubmerchantMcc.Trim()}</SubmerchantMCC>");
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
