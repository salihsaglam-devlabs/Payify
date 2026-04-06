using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpVoidRequest : IvpRequestBase
{
    public string OrgOrderId { get; set; }
    public string OrderId { get; set; }
    public int Currency { get; set; }
    public int Moto { get; set; } = 0;

    public string BuildRequest()
    {
        var formBuilder = new StringBuilder();
        formBuilder.AppendFormat("ShopCode={0}&", ShopCode);
        formBuilder.AppendFormat("Currency={0}&", Currency);
        formBuilder.AppendFormat("TxnType={0}&", TxnType);
        formBuilder.AppendFormat("OrderId={0}&", OrderId);
        formBuilder.AppendFormat("orgOrderId={0}&", OrgOrderId);
        formBuilder.AppendFormat("UserCode={0}&", UserCode);
        formBuilder.AppendFormat("UserPass={0}&", UserPass);
        formBuilder.AppendFormat("SecureType={0}&", SecureType);
        formBuilder.AppendFormat("MOTO={0}&", Moto);
        formBuilder.AppendFormat("Lang={0}", Lang.ToUpper());

        return formBuilder.ToString();
    }
}
