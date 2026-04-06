using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.PF.Domain.Entities;

public class Currency : AuditEntity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public int Number { get; set; }
    public CurrencyType CurrencyType { get; set; }
}
