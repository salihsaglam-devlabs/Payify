using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class BankLogo : AuditEntity
{
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }

    [ForeignKey("Bank")]
    public Guid BankId { get; set; }
    public Bank Bank { get; set; }
}
