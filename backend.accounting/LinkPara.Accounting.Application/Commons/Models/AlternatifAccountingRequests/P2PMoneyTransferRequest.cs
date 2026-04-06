
namespace LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests;

public class P2PMoneyTransferRequest
{
    public DateTime islemTarihi { get; set; }
    public string islemNo { get; set; }
    public decimal tutar { get; set; }
    public string dovizTuru { get; set; }
    public decimal dovizKuru { get; set; }
    public string aciklama { get; set; }
    public string komisyonOdemeSekli { get; set; }
    public decimal komisyonGonderenTutari { get; set; }
    public decimal komisyonAliciTutari { get; set; }
    public CustomerRequest musteri { get; set; }
    public CustomerRequest musteriAlici { get; set; }

}