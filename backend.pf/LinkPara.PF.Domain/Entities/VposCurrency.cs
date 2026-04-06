using LinkPara.SharedModels.Persistence;
namespace LinkPara.PF.Domain.Entities;
public class VposCurrency : AuditEntity
{
    public Guid VposId { get; set; }
    public Vpos Vpos { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
}
