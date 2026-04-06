using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpPaymentDetailRequest : IvpRequestBase
{
    public string OrgOrderId { get; set; }

    public string BuildRequest()
    {
        var formBuilder = new StringBuilder();
        formBuilder.AppendFormat("ShopCode={0}&", ShopCode);
        formBuilder.AppendFormat("TxnType={0}&", TxnType);
        formBuilder.AppendFormat("orgOrderId={0}&", OrgOrderId);
        formBuilder.AppendFormat("UserCode={0}&", UserCode);
        formBuilder.AppendFormat("UserPass={0}&", UserPass);
        formBuilder.AppendFormat("SecureType={0}&", SecureType);
        formBuilder.AppendFormat("Lang={0}", Lang.ToUpper());

        return formBuilder.ToString();
    }
}
