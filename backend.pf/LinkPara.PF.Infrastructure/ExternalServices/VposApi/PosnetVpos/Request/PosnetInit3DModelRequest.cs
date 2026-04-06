using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetInit3DModelRequest : PosnetPaymentBase
    {
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<oosRequestData>");
            requestXml.AppendLine($"<posnetid>{PosnetId}</posnetid>");
            requestXml.AppendLine($"<XID>{OrderId}</XID>");
            requestXml.AppendLine($"<amount>{Amount}</amount>");
            requestXml.AppendLine($"<currencyCode>{CurrencyCode}</currencyCode>");

            if (NumberOfInstallments > 1)
            {
                string formattedInstallment = NumberOfInstallments < 10 ? $"0{NumberOfInstallments}" : NumberOfInstallments.ToString();
                requestXml.AppendLine($"<installment>{formattedInstallment}</installment>");
            }
            else
            {
                requestXml.AppendLine($"<installment>00</installment>");
            }

            requestXml.AppendLine($"<tranType>{TransactionType}</tranType>");
            requestXml.AppendLine($"<cardHolderName>{CardHolderName}</cardHolderName>"); 
            requestXml.AppendLine($"<ccno>{CardNo}</ccno>");
            requestXml.AppendLine($"<cvc>{Cvv.Trim()}</cvc>");
            requestXml.AppendLine($"<expDate>{ExpireDate}</expDate>");
            requestXml.AppendLine($"</oosRequestData>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
