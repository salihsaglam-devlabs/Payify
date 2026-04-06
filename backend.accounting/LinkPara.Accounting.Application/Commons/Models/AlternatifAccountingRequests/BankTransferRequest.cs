
namespace LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests;

public class BankTransferRequest
{
    public DateTime islemTarihi { get; set; }
    public string bankaIslemTipi { get; set; }
    public string islemNo { get; set; }
    public string bankaHesapNo { get; set; }
    public string bankaHesapAdi { get; set; }
    public decimal tutar { get; set; }
    public string dovizTuru { get; set; }
    public decimal dovizKuru { get; set; }
    public string aciklama { get; set; }
    public decimal masrafTutari { get; set; }
    public CustomerRequest musteri { get; set; }
}