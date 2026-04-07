namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class AccountInfoConsentResultDto
{
    public ConsentInformation RzBlg { get; set; }
    public AccountGkdInformation Gkd { get; set; }
    public ParticipantInformation KatilimciBlg { get; set; }
    public ConsentAccountInformation HspBlg { get; set; }
}

public class ConsentInformation
{
    public string RizaNo { get; set; }
    public DateTime OlusZmn { get; set; }
    public DateTime GnclZmn { get; set; }
    public string RizaDrm { get; set; }
    public string RizaIptDtyKod{ get; set; }
}
public class AccountGkdInformation
{
    public string YetYntm { get; set; }
    public string YonAdr { get; set; }
    public DateTime YetTmmZmn { get; set; }
    public string HhsYonAdr { get; set; }
    public AyrikGkdType AyrikGkd { get; set; }

}

public class AyrikGkdType
{
    public string OhkTanimTip { get; set; }
    public string OhkTanimDeger { get; set; }

}

public class ParticipantInformation
{
    public string HhsKod { get; set; }
    public string YosKod { get; set; }

}

public class ConsentAccountInformation
{
    public PermissionInformation IznBlg { get; set; }
    public ConsentDetailInformation AyrBlg { get; set; }
    public AccountConsentIdentityInfo Kmlk { get; set; }

}

public class PermissionInformation
{
    public List<string> IznTur { get; set; }
    public DateTime ErisimIzniSonTrh { get; set; }
    public DateTime HesapIslemBslZmn { get; set; }
    public DateTime HesapIslemBtsZmn { get; set; }
    

}
public class ConsentDetailInformation
{
    public string OhkMsj { get; set; }

}

