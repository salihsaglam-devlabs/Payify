using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetPostAuthRequest : PosnetRequestBase
    {
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public int NumberOfInstallments { get; set; }
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<capt>");
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
            requestXml.AppendLine($"<orderID>{OrderId}</orderID>");
            requestXml.AppendLine($"<orderDate>{OrderDate}</orderDate>");
            requestXml.AppendLine($"</capt>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
