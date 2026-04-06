using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    internal class PosnetPaymentNonSecureRequest: PosnetPaymentBase
    {
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<tranDateRequired>{TranDateRequired}</tranDateRequired>");
            string txnType = String.Equals(TransactionType, "Auth") ? "auth" : "sale"; 
            requestXml.AppendLine($"<{txnType}>");
            requestXml.AppendLine($"<amount>{Amount}</amount>");
            requestXml.AppendLine($"<ccno>{CardNo}</ccno>");
            requestXml.AppendLine($"<currencyCode>{CurrencyCode}</currencyCode>");
            requestXml.AppendLine($"<cvc>{Cvv.Trim()}</cvc>");
            requestXml.AppendLine($"<expDate>{ExpireDate}</expDate>");
            requestXml.AppendLine($"<orderID>{OrderId}</orderID>");

            if (NumberOfInstallments > 1)
            {
                string formattedInstallment = NumberOfInstallments < 10 ? $"0{NumberOfInstallments}" : NumberOfInstallments.ToString();
                requestXml.AppendLine($"<installment>{formattedInstallment}</installment>");
            }
            else
            {
                requestXml.AppendLine($"<installment>00</installment>");
            }

            requestXml.AppendLine($"<subMrcId>{SubMerchantId.Trim()}</subMrcId>");
            requestXml.AppendLine($"<mrcPfId>{MrcPfId}</mrcPfId>");
            requestXml.AppendLine($"<Mcc>{Mcc}</Mcc>");

            if (!String.IsNullOrEmpty(Tckn))
                requestXml.AppendLine($"<tckn>{Tckn}</tckn>");
            if (!String.IsNullOrEmpty(Vkn))
                requestXml.AppendLine($"<vkn>{Vkn}</vkn>");
            if (!String.IsNullOrEmpty(SubDealerCode))
                requestXml.AppendLine($"<subDealerCode>{SubDealerCode}</subDealerCode>");
            requestXml.AppendLine($"</{txnType}>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
