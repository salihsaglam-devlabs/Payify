using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Domain.Entities;

public class Currency : AuditEntity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public CurrencyType CurrencyType { get; set; }    
}