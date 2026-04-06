using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.PF.Domain.Entities;

public class Bank : AuditEntity
{
    public int Code { get; set; }
    public string Name { get; set; }
}
