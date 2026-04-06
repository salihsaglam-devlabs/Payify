
namespace LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests;

public class RegisterInvoiceRequest
{
    public CustomerRequest musteri { get; set; }
    public DateTime islemTarihi { get; set; }
    public string islemSaati { get; set; }
    public string islemNo { get; set; }
    public decimal islemTutari => islemKomisyonTutari + bsmvToplamTutar;
    public string islemDovizTuru { get; set; }
    public string islemDovizKuru { get; set; }
    public decimal bsmvToplamTutar { get; set; }
    public decimal islemKomisyonTutari { get; set; }
    public string aciklama { get; set; }
    public List<CommissionDetailRequest> komisyonDetaylari { get; set; }
}

public class CommissionDetailRequest
{
    public string islemKodu { get; set; }
    public string islemAciklama { get; set; }
    public string dovizTuru { get; set; }
    public decimal tutar { get; set; }
}