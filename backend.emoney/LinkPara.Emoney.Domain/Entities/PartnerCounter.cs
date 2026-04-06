
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Emoney.Domain.Entities;

public class PartnerCounter : AuditEntity
{
    public int Index { get; set; }
}
