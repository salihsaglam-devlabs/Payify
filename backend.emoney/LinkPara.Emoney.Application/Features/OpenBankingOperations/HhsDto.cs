namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class HhsDto
{
    public string Kod { get; set; }
    public string Unv { get; set; }
    public string Marka { get; set; }
    public string AcikAnahtar { get; set; }
    public string Durum { get; set; }
    public List<HhsApiInfoType> ApiBilgileri { get; set; }
    public List<HhsLogoInfoType> LogoBilgileri { get; set; }
}

public class HhsApiInfoType
{
    public string Api { get; set; }
    public string Surum { get; set; }

}

public class HhsLogoInfoType
{
    public string LogoTur { get; set; }
    public string LogoAdr { get; set; }

}