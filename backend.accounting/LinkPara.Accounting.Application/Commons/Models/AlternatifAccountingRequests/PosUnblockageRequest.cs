namespace LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests;

public class PosUnblockageRequest
{
    public DateTime islemTarihi { get; set; }
    public string islemNo { get; set; }
    public string bankaHesapNo { get; set; }
    public string bankaHesapAdi { get; set; }
    public string posHesapNo { get; set; }
    public string posHesapAdi { get; set; }
    public decimal tutar { get; set; }
    public string dovizTuru { get; set; }
    public decimal dovizKuru { get; set; }
    public string aciklama { get; set; }
    public decimal masrafTutari { get; set; }
}
