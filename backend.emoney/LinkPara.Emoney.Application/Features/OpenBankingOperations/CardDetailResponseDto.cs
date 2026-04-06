namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class CardDetailResponseDto
{    
    public string KartRef { get; set; }
    public int DonemDegeri { get; set; }
    public DateTime DonemBaslangicTarihi { get; set; }
    public DateTime DonemBitisTarihi { get; set; }
    public string EkstreTuru { get; set; }
    public AmountCurrencyInformation EkstreBorcu { get; set; }
    public AmountCurrencyInformation AsgariOdemeTutari { get; set; }
    public AmountCurrencyInformation DonemTaksitTutari { get; set; }
    public DateTime HesapKesimTarihi { get; set; }
    public DateTime SonOdemeTarihi { get; set; }
    public List<TransactionInformation> HareketBilgileri { get; set; }
    public AmountCurrencyInformation OrijinalIslemTutari { get; set; }
    public DateTime IslemTarihi { get; set; }
    public string BorcAlacak { get; set; }
    public string IslemAciklamasi { get; set; }
    public List<TransactionPointInformation> IslemPuanBilgileri { get; set; }
    public AmountCurrencyInformation ToplamTaksitTutari { get; set; }
    public int ToplamTaksitSayisi { get; set; }
    public int TaksitDonemi { get; set; }
    public string SaticiKategoriKodu { get; set; }
    
}

public class TransactionPointInformation
{
    public string IslemPuani { get; set; }
    public string IslemPuanBirimi { get; set; }
    public string IslemPuanDurumu { get; set; }
}

public class TransactionInformation
{
    public string IslemNo { get; set; }
    public AmountCurrencyInformation IslemTutari { get; set; }
}

