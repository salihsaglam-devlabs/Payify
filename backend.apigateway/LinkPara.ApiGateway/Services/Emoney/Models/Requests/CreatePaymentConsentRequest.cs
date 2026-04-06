using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class CreatePaymentConsentRequest
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public YosForwardType YonTipi { get; set; }
    public string OhkTur { get; set; }
    public string DrmKod { get; set; }
    public ProcessAmountOb IslTtr { get; set; }
    public RetailPersonOb Alc { get; set; }
    public QrPaymentOb Kkod { get; set; }
    public RetailPersonOb Gon { get; set; }
    public PaymentConsentDetailOb OdmAyr { get; set; }
    public PaymentConsentCompanyDto IsyOdmBlg { get; set; }
    public PaymentConsentInformationDto OdmBsltm { get; set; }

}

public class ProcessAmountOb
{
    public string PrBrm { get; set; }
    public string Ttr { get; set; }
}

public class RetailPersonOb
{
    public string HspNo { get; set; }
    public string Unv { get; set; }
    public string HspRef { get; set; }
    public KolasInfo Kolas { get; set; }
}
public class KolasInfo
{    
    public string KolasTur { get; set; }
    public string KolasDgr { get; set; }
    public string KolasRefNo { get; set; }
    public string KolasHspTur { get; set; }
}
public class QrPaymentOb
{
    public string AksTur { get; set; }
    public string KkodRef { get; set; }
    public string KkodUrtcKod { get; set; }
}

public class PaymentConsentDetailOb
{
    public string OdmKynk { get; set; }
    public string OdmAmc { get; set; }
    public string OdmAclkm { get; set; }
    public string RefBlg { get; set; }
    
}

public class PaymentConsentCompanyDto
{
    public string IsyKtgKod { get; set; }
    public string AltIsyKtgGod { get; set; }
    public string GenelUyeIsyeriNo { get; set; }
}
public class PaymentConsentInformationDto
{
    public AccountConsentIdentityInfo Kmlk { get; set; }
}
