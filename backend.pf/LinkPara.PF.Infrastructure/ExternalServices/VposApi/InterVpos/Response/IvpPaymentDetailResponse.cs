using System.Globalization;
using System.Web;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Response;

public class IvpPaymentDetailResponse : IvpResponseBase
{
    public string OrderId { get; set; }
    public string BatchNo { get; set; }

    /// <summary>
    /// Ýþlem durumu: 
    /// N:Normal
    /// V:Ýptal
    /// R:Reversed
    /// S:Þüpheli
    /// </summary>
    public string TxnStat { get; set; }

    public decimal PurchAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public DateTime? VoidDate { get; set; }

    /// <summary>
    /// D: Reddedilmiþ Ýþlem
    /// V: Ýptal Edilmiþ
    /// A: Ön provizyon
    /// K: Kapamasý yapýlmýþ önprovizyon
    /// S: Günsonu Yapýlmýþ
    /// C: Günsonu Yapýlmamýþ
    /// </summary>
    public string TxnStatus { get; set; }

    /// <summary>
    /// S: Satýþ
    /// C: Iade
    /// </summary>
    public string ChargeTypeCd { get; set; }

    public string ErrorCode { get; set; }

    public IvpPaymentDetailResponse Parse(string response)
    {
        response = response.Replace(";;", ";").Replace(";", "&");

        var responseParams = HttpUtility.ParseQueryString(response);

        var vd = DateTime.TryParse(responseParams["VoidDate"], out var voidDate);

        TransId = responseParams["TransId"];
        OrderId = responseParams["OrderId"];
        BatchNo = responseParams["BatchNo"];     
        VoidDate = vd ? voidDate : null;
        ChargeTypeCd = responseParams["ChargeTypeCd"];
        PurchAmount = decimal.Parse(responseParams["PurchAmount"]);
        RefundedAmount = decimal.Parse(responseParams["RefundedAmount"]);
        ErrorCode = responseParams["ErrorCode"];
        ErrorMessage = responseParams["ErrorMessage"];
        ProcReturnCode = responseParams["ProcReturnCode"];
        TxnStatus = responseParams["TxnStatus"];
        TxnStat = responseParams["TxnStat"];

        if (!string.IsNullOrEmpty(responseParams["TrxDate"]))
            TrxDate = DateTime.ParseExact(responseParams["TrxDate"], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        return this;
    }
}
