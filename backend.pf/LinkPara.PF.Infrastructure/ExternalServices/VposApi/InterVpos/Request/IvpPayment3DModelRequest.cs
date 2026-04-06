using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpPayment3DModelRequest : IvpPaymentBase
{
    public string PayerAuthenticationCode { get; set; }
    public string Eci { get; set; }
    public string PayerTxnId { get; set; }
    public string MD { get; set; }

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
        formBuilder.AppendFormat("InstallmentCount={0}&", InstallmentCount);
        formBuilder.AppendFormat("Currency={0}&", Currency);
        formBuilder.AppendFormat("SubMerchantCode={0}&", SubMerchantCode);
        formBuilder.AppendFormat("PurchAmount={0}&", PurcAmount);
        formBuilder.AppendFormat("BonusAmount={0}&", BonusAmount);
        formBuilder.AppendFormat("PayerAuthenticationCode={0}&", PayerAuthenticationCode);
        formBuilder.AppendFormat("Eci={0}&", Eci);
        formBuilder.AppendFormat("PayerTxnId={0}&", PayerTxnId);
        formBuilder.AppendFormat("MD={0}&", MD);

        return formBuilder.ToString();
    }
}
