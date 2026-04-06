using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpInit3dModelRequest : IvpPaymentBase
{
    public string MerchantPass { get; set; }
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public string Rnd { get; set; }
    public string Hash { get; set; }

    public string BuildRequest()
    {
        var formBuilder = new StringBuilder();

        formBuilder.AppendFormat("ShopCode={0}&", ShopCode);
        formBuilder.AppendFormat("TxnType={0}&", TxnType);
        formBuilder.AppendFormat("OrderId={0}&", OrderId);        
        formBuilder.AppendFormat("SecureType={0}&", SecureType);
        formBuilder.AppendFormat("Lang={0}&", Lang.ToUpper());
        formBuilder.AppendFormat("InstallmentCount={0}&", InstallmentCount);
        formBuilder.AppendFormat("Currency={0}&", Currency);
        formBuilder.AppendFormat("SubMerchantCode={0}&", SubMerchantCode);
        formBuilder.AppendFormat("PurchAmount={0}&", PurcAmount);
        formBuilder.AppendFormat("Pan={0}&", Pan);
        formBuilder.AppendFormat("Expiry={0}&", Expiry);
        formBuilder.AppendFormat("Cvv2={0}&", Cvv2);
        formBuilder.AppendFormat("BonusAmount={0}&", BonusAmount);
        formBuilder.AppendFormat("Rnd={0}&", Rnd);
        formBuilder.AppendFormat("Hash={0}&", Hash);
        formBuilder.AppendFormat("OkUrl={0}&", OkUrl);
        formBuilder.AppendFormat("FailUrl={0}&", FailUrl);
        formBuilder.AppendFormat("Version3D=2.0");

        return formBuilder.ToString();
    }
}
