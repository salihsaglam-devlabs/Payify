namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class CardsResponseDto
{
    public string RizaNo { get; set; }
    public string KartRef { get; set; }
    public string KartNo { get; set; }
    public string AsilKartNo { get; set; }
    public string KartTipi { get; set; }
    public string AltKartTipi { get; set; }
    public string KartFormu { get; set; }
    public string KartTuru { get; set; }
    public string KartStatu { get; set; }
    public string KartSahibi { get; set; }
    public string KartTicariUnvan { get; set; }
    public string KartUrunAdi { get; set; }
    public List<StatementType> EkstreTurleri { get; set; }    
    public string KartRumuz { get; set; }
    public string KartSema { get; set; }
}

public class StatementType
{    
    public string EkstreStatu { get; set; }
    public string ParaBirimi { get; set; }
}

