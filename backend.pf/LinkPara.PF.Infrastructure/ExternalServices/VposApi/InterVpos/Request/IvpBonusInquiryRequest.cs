using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpBonusInquiryRequest : IvpPaymentBase
{
    public string BuildRequest()
    {
        var formBuilder = new StringBuilder();

        formBuilder.AppendFormat("ShopCode={0}&", ShopCode);
        formBuilder.AppendFormat("TxnType={0}&", TxnType);
        formBuilder.AppendFormat("OrderId={0}&", OrderId);
        formBuilder.AppendFormat("UserCode={0}&", UserCode);
        formBuilder.AppendFormat("UserPass={0}&", UserPass);
        formBuilder.AppendFormat("SecureType={0}&", SecureType);
        formBuilder.AppendFormat("Lang={0}&", Lang.ToUpper());
        formBuilder.AppendFormat("Currency={0}&", Currency);
        formBuilder.AppendFormat("Pan={0}&", Pan);
        formBuilder.AppendFormat("Expiry={0}&", Expiry);
        formBuilder.AppendFormat("Cvv2={0}&", Cvv2);        
        return formBuilder.ToString();
    }
}
