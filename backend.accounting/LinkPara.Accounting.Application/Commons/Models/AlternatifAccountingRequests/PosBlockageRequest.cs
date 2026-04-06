using LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests.Enums;

namespace LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests;

public class PosBlockageRequest
{
    public PostBlockageType sanalPosIslemTipi { get; set; }
    public DateTime islemTarihi { get; set; }
    public string islemSaati { get; set; }
    public string islemNo { get; set; }
    public decimal islemTutari { get; set; }
    public string islemBankaKodu { get; set; }
    public string islemBankaHesapNo { get; set; }
    public int islemTaksitSayisi { get; set; }
    public string aciklama { get; set; }
    public CustomerRequest musteri { get; set; }
    public string islemNoKontrol { get; set; }
}