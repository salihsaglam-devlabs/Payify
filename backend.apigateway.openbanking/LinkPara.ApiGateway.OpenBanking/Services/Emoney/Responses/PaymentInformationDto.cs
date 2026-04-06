namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class PaymentInformationDto
{
    public IdentityInfoDto Kmlk { get; set; }
    public PriceInfo IslTtr { get; set; }
    public PersonInfo Gon { get; set; }
    public PersonInfo Alc { get; set; }
    public QrCodeInfo Kkod { get; set; }
    public PaymentDetail OdmAyr { get; set; }
}

public class PriceInfo
{
    public string PrBrm { get; set; }
    public string Ttr { get; set; }

}

public class PersonInfo
{
    public string Unv { get; set; }
    public string HspNo { get; set; }
    public string HspRef { get; set; }
    public KolasInfoType Kolas { get; set; }

}

public class KolasInfoType
{
    public string KolasTur { get; set; }
    public string KolasDgr { get; set; }
    public string KolasRefNo { get; set; }
    public string KolasHspTur { get; set; }

}

public class QrCodeInfo
{
    public string AksTur { get; set; }
    public string KkodRef { get; set; }
    public string KkodUrtcKod { get; set; }

}

public class PaymentDetail
{
    public string OdmKynk { get; set; }
    public string OdmAmc { get; set; }
    public string RefBlg { get; set; }
    public string OdmAcklm { get; set; }
    public string OhkMsj { get; set; }
    public string OdmStm { get; set; } 
    public string OdmStmNo { get; set; } 
    public string BekOdmZmn { get; set; } 
}