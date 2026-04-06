
using LinkPara.Accounting.Domain.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Accounting.Domain.Entities;
public class Template : AuditEntity, ITrackChange
{
    public OperationType OperationType { get; set; }
    public bool HasCommission { get; set; }
    /// <summary>
    /// 1	Cüzdana Para Yatırma
    /// 2	Komisyonsuz para transferi(Rubik to Rubik)
    /// 3	Komisyonlu Para Transferi(Rubik to Rubik)
    /// </summary>
    public int ExternalOperationType { get; set; }
    /// <summary>
    ///  VRM= Finans/Cari Hesap Fisleri/Virman Fis, 
    ///  SAT_SIP = Satis Dagitim/Satis Siparisi, 
    ///  AL_SIP = Satin Alma/Alim Siparisi, 
    ///  SAT_FAT = Satis Dagitim/Verilen Hizmet Faturasi
    /// </summary>
    public string TranCode { get; set; }
    /// <summary>
    /// B:Borç, 
    /// A:Alacak olarak hesaba isletir.
    /// </summary>
    public string Direction { get; set; }
    public string AccountNumber { get; set; }
    public TemplateExpenseType TemplateExpenseType { get; set; }
    public string SrvCode { get; set; }
}
